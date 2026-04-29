using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Domain.Enums;
using PitStop.Infrastructure.Data;
using PitStop.Infrastructure.Identity;
using PitStop.Infrastructure.Repositories;
using PitStop.Infrastructure.Services;
using PitStop.Infrastructure.Storage;
using PitStop.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// ─── Razor Components ─────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── Database ─────────────────────────────────────────────────────────────────
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Postgres";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")
            ?? "Data Source=pitstop_dev.db");
    else
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ─── Identity ─────────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ─── Cookie Auth ──────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.AccessDeniedPath = "/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// ─── External Auth (Google) ───────────────────────────────────────────────────
var googleClientId     = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && googleClientId != "YOUR_GOOGLE_CLIENT_ID")
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId     = googleClientId;
            options.ClientSecret = googleClientSecret!;
            options.CallbackPath = "/auth/google-callback";
        });
}

// ─── Authorization Policies ───────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ShopOwner", policy => policy.RequireRole("ShopOwner"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// ─── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IShopRequestRepository, ShopRequestRepository>();
builder.Services.AddScoped<IFavoriteShopRepository, FavoriteShopRepository>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

await SeedRolesAsync(app.Services);
await SeedAdminAsync(app.Services);
await SeedUsersAsync(app.Services);
await SeedShopsAsync(app.Services);

// ─── Middleware Pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticFiles(); // serves wwwroot/uploads/ and other static content

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/do-logout", async (HttpContext ctx, SignInManager<ApplicationUser> signInMgr) =>
{
    await signInMgr.SignOutAsync();
    return Results.Redirect("/");
});

app.MapPost("/auth/do-set-password", async (HttpContext ctx, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signInMgr) =>
{
    var form           = await ctx.Request.ReadFormAsync();
    var userId         = form["userId"].ToString();
    var token          = form["token"].ToString();
    var password       = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();

    if (password != confirmPassword)
        return Results.Redirect($"/auth/set-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}&err=mismatch");

    var user = await userMgr.FindByIdAsync(userId);
    if (user is null)
        return Results.Redirect($"/auth/set-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}&err=invalid");

    var result = await userMgr.ResetPasswordAsync(user, token, password);
    if (!result.Succeeded)
    {
        var errCode = result.Errors.Any(e => e.Code.Contains("Password")) ? "weak" : "invalid";
        return Results.Redirect($"/auth/set-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}&err={errCode}");
    }

    await signInMgr.SignInAsync(user, isPersistent: false);
    return Results.Redirect("/shop/dashboard");
});

app.MapGet("/auth/google-login", (HttpContext ctx) =>
{
    var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = "/auth/google-callback"
    };
    return Results.Challenge(props, ["Google"]);
});

app.MapGet("/auth/google-callback", async (HttpContext ctx, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signInMgr) =>
{
    var info = await signInMgr.GetExternalLoginInfoAsync();
    if (info is null)
        return Results.Redirect("/auth/login?error=invalid");

    // Try to sign in with existing external login link
    var signInResult = await signInMgr.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
    if (signInResult.Succeeded)
        return Results.Redirect("/");

    // No existing link — find or create user by email
    var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var name  = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email ?? "Utilizator";
    if (string.IsNullOrWhiteSpace(email))
        return Results.Redirect("/auth/login?error=invalid");

    var user = await userMgr.FindByEmailAsync(email);
    if (user is null)
    {
        user = new ApplicationUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true, CreatedAt = DateTime.UtcNow };
        var createResult = await userMgr.CreateAsync(user);
        if (!createResult.Succeeded)
            return Results.Redirect("/auth/login?error=invalid");
        await userMgr.AddToRoleAsync(user, "User");
    }

    await userMgr.AddLoginAsync(user, info);
    await signInMgr.SignInAsync(user, isPersistent: false);
    return Results.Redirect("/");
});

app.MapPost("/auth/do-login", async (HttpContext ctx, SignInManager<ApplicationUser> signInMgr) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var rememberMe = form["rememberMe"].ToString() == "true";
    var returnUrl = form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return Results.Redirect("/auth/login?error=empty");

    var result = await signInMgr.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
    if (!result.Succeeded)
        return Results.Redirect("/auth/login?error=invalid");

    // Redirect to returnUrl only if it's a local path (prevent open redirect)
    var destination = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/') ? returnUrl : "/";
    return Results.Redirect(destination);
});

app.MapPost("/auth/do-register", async (HttpContext ctx, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signInMgr) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var name = form["name"].ToString();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();
    var agreeTerms = form["agreeTerms"].ToString() == "true";

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return Results.Redirect("/auth/login?tab=register&regError=empty");
    if (password != confirmPassword)
        return Results.Redirect("/auth/login?tab=register&regError=mismatch");
    if (!agreeTerms)
        return Results.Redirect("/auth/login?tab=register&regError=terms");

    var user = new ApplicationUser { UserName = email, Email = email, FullName = name, CreatedAt = DateTime.UtcNow };
    var result = await userMgr.CreateAsync(user, password);
    if (!result.Succeeded)
    {
        var code = result.Errors.Any(e => e.Code.Contains("Duplicate")) ? "exists" : "weak";
        return Results.Redirect($"/auth/login?tab=register&regError={code}");
    }
    await userMgr.AddToRoleAsync(user, "User");
    await signInMgr.SignInAsync(user, isPersistent: false);
    return Results.Redirect("/");
});

app.Run();

// ─── User Seeding ─────────────────────────────────────────────────────────────
static async Task SeedUsersAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var users = new[]
    {
        ("Andrei Popescu",     "andrei.popescu@gmail.com"),
        ("Maria Ionescu",      "maria.ionescu@yahoo.com"),
        ("Bogdan Constantin",  "bogdan.constantin@gmail.com"),
        ("Elena Dumitrescu",   "elena.dumitrescu@gmail.com"),
        ("Mihai Stanescu",     "mihai.stanescu@outlook.com"),
        ("Ioana Georgescu",    "ioana.georgescu@gmail.com"),
        ("Alexandru Marin",    "alex.marin@yahoo.com"),
        ("Cristina Florescu",  "cristina.florescu@gmail.com"),
        ("Radu Popa",          "radu.popa@gmail.com"),
        ("Ana Stoica",         "ana.stoica@yahoo.com"),
    };

    foreach (var (name, email) in users)
    {
        if (await userManager.FindByEmailAsync(email) is not null) continue;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = name,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };
        var result = await userManager.CreateAsync(user, "Parola1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "User");
    }
}

// ─── Shop Seeding ─────────────────────────────────────────────────────────────
static async Task SeedShopsAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await db.Shops.AnyAsync()) return;

    static List<ShopHour> WeekdayHours(TimeOnly open, TimeOnly close, bool saturdayOpen = true) =>
        Enumerable.Range(0, 7).Select(d => new ShopHour
        {
            DayOfWeek = d,
            IsClosed = d == 6 || (d == 5 && !saturdayOpen),
            OpenTime  = (d == 6 || (d == 5 && !saturdayOpen)) ? (TimeOnly?)null : open,
            CloseTime = (d == 6 || (d == 5 && !saturdayOpen)) ? (TimeOnly?)null : close,
        }).ToList();

    var shops = new List<Shop>
    {
        new()
        {
            Name = "Auto Expert Cluj",
            Description = "Service auto autorizat cu experiență de peste 15 ani. Oferim reparații complete pentru orice marcă de autoturism, diagnosticare computerizată și revizie tehnică.",
            Address = "Str. Fabricii nr. 42",
            City = "Cluj-Napoca",
            County = "Cluj",
            Phone = "0264-123456",
            Email = "contact@autoexpertcluj.ro",
            Website = "https://autoexpertcluj.ro",
            Category = ShopCategory.ServiceAuto,
            Status = ShopStatus.Active,
            AverageRating = 4.7,
            ReviewCount = 3,
            Hours = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(18, 0)),
            Services =
            [
                new ShopService { Name = "Schimb ulei și filtre", Description = "Schimb ulei motor, filtru ulei, filtru aer, filtru habitaclu", PriceMin = 150, PriceMax = 350 },
                new ShopService { Name = "Diagnosticare computerizată", Description = "Citire erori OBD2, resetare martori bord", PriceMin = 80, PriceMax = 80 },
                new ShopService { Name = "Frâne față/spate", Description = "Înlocuire plăcuțe și discuri de frână", PriceMin = 200, PriceMax = 600 },
                new ShopService { Name = "Distribuție", Description = "Înlocuire curea sau lanț de distribuție", PriceMin = 500, PriceMax = 1200 },
                new ShopService { Name = "Revizie tehnică completă", Description = "Revizie periodică conform normelor producătorului", PriceMin = 300, PriceMax = 700 },
            ],
        },
        new()
        {
            Name = "Vulcanizare Non-Stop Titan",
            Description = "Vulcanizare rapidă non-stop în București. Montaj anvelope, echilibrare, geometrie roți și reparații urgente.",
            Address = "Bd. Iuliu Maniu nr. 15",
            City = "București",
            County = "Ilfov",
            Phone = "0721-456789",
            Email = "titan@vulcanizare.ro",
            Category = ShopCategory.Vulcanizare,
            Status = ShopStatus.Active,
            AverageRating = 4.3,
            ReviewCount = 2,
            Hours = Enumerable.Range(0, 7).Select(d => new ShopHour
            {
                DayOfWeek = d,
                IsClosed = false,
                OpenTime = new TimeOnly(0, 0),
                CloseTime = new TimeOnly(23, 59),
            }).ToList(),
            Services =
            [
                new ShopService { Name = "Montaj anvelopă", Description = "Demontare și montare anvelopă pe jantă", PriceMin = 20, PriceMax = 40 },
                new ShopService { Name = "Echilibrare roată", Description = "Echilibrare dinamică cu aparat computerizat", PriceMin = 15, PriceMax = 25 },
                new ShopService { Name = "Geometrie roți 3D", Description = "Reglaj geometrie față-spate cu echipament 3D", PriceMin = 120, PriceMax = 180 },
                new ShopService { Name = "Reparație pană", Description = "Reparație pană cu spanac sau petic interior", PriceMin = 30, PriceMax = 60 },
            ],
        },
        new()
        {
            Name = "Spălătorie Auto Premium Timișoara",
            Description = "Spălătorie auto manuală și automată. Curățenie interioară completă, polish și tratamente ceramice pentru protecție de lungă durată.",
            Address = "Calea Șagului nr. 88",
            City = "Timișoara",
            County = "Timiș",
            Phone = "0256-789012",
            Email = "office@spalatoriepremium.ro",
            Website = "https://spalatoriepremium.ro",
            Category = ShopCategory.Spalatorie,
            Status = ShopStatus.Active,
            AverageRating = 4.5,
            ReviewCount = 2,
            Hours = WeekdayHours(new TimeOnly(7, 30), new TimeOnly(20, 0), saturdayOpen: true),
            Services =
            [
                new ShopService { Name = "Spălare exterioară manuală", Description = "Spălare caroserie, jante și geamuri manual", PriceMin = 50, PriceMax = 80 },
                new ShopService { Name = "Curățenie interior completă", Description = "Aspirare, șamponare tapițerie și plastic", PriceMin = 150, PriceMax = 300 },
                new ShopService { Name = "Polish caroserie", Description = "Înlăturare zgârieturi fine, refacere luciu", PriceMin = 400, PriceMax = 900 },
                new ShopService { Name = "Tratament ceramic", Description = "Aplicare coating ceramic pentru protecție 2 ani", PriceMin = 800, PriceMax = 1500 },
                new ShopService { Name = "Ozon interior", Description = "Tratament dezinfectare cu ozon a habitaclului", PriceMin = 80, PriceMax = 80 },
            ],
        },
        new()
        {
            Name = "Vopsitorie Auto Brașov Color",
            Description = "Vopsitorie auto profesională cu cabină de vopsit modernă. Reparații caroserie, tinichigerie și vopsire cu garanție 3 ani.",
            Address = "Str. Lungă nr. 120",
            City = "Brașov",
            County = "Brașov",
            Phone = "0268-345678",
            Email = "contact@brasovcolor.ro",
            Category = ShopCategory.Vopsitorie,
            Status = ShopStatus.Active,
            AverageRating = 4.8,
            ReviewCount = 1,
            Hours = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 0), saturdayOpen: false),
            Services =
            [
                new ShopService { Name = "Vopsire panou", Description = "Vopsire aripă, ușă sau capotă cu vopsea originală", PriceMin = 300, PriceMax = 600 },
                new ShopService { Name = "Reparație zgârietură", Description = "Reparație locală scratch sau lovitură mică", PriceMin = 100, PriceMax = 250 },
                new ShopService { Name = "Îndreptare caroserie", Description = "Tinichigerie și îndreptare table cu extractor PDR", PriceMin = 150, PriceMax = 500 },
                new ShopService { Name = "Vopsire integrală", Description = "Revopsire completă autoturism în culoarea dorită", PriceMin = 2500, PriceMax = 6000 },
            ],
        },
        new()
        {
            Name = "ITP Rapid Iași",
            Description = "Stație ITP autorizată RAR. Inspecție tehnică periodică rapidă, fără programare, în maximum 45 minute.",
            Address = "Str. Nicolina nr. 33",
            City = "Iași",
            County = "Iași",
            Phone = "0232-567890",
            Email = "itp@rapid-iasi.ro",
            Category = ShopCategory.ITP,
            Status = ShopStatus.Active,
            AverageRating = 4.2,
            ReviewCount = 2,
            Hours = WeekdayHours(new TimeOnly(7, 0), new TimeOnly(19, 0), saturdayOpen: true),
            Services =
            [
                new ShopService { Name = "ITP autoturism", Description = "Inspecție tehnică periodică autoturism categoria M1", PriceMin = 140, PriceMax = 140 },
                new ShopService { Name = "ITP autoutilitară", Description = "ITP vehicule utilitare până la 3.5t", PriceMin = 180, PriceMax = 220 },
                new ShopService { Name = "Verificare emisii", Description = "Control gaze evacuare conform normelor Euro", PriceMin = 50, PriceMax = 50 },
            ],
        },
        new()
        {
            Name = "Piese Auto Dedeman Constanța",
            Description = "Magazin de piese auto cu stoc larg pentru autoturisme europene și asiatice. Livrare rapidă și consiliere tehnică gratuită.",
            Address = "Bd. Tomis nr. 210",
            City = "Constanța",
            County = "Constanța",
            Phone = "0241-678901",
            Email = "vanzari@pieseauto-constanta.ro",
            Website = "https://pieseauto-constanta.ro",
            Category = ShopCategory.PieseAuto,
            Status = ShopStatus.Active,
            AverageRating = 4.4,
            ReviewCount = 1,
            Hours = WeekdayHours(new TimeOnly(8, 30), new TimeOnly(18, 30), saturdayOpen: true),
            Services =
            [
                new ShopService { Name = "Piese originale OEM", Description = "Piese originale producător pentru orice marcă", PriceMin = 50, PriceMax = 2000 },
                new ShopService { Name = "Piese aftermarket", Description = "Piese aftermarket certificate calitate OE", PriceMin = 20, PriceMax = 800 },
                new ShopService { Name = "Consumabile auto", Description = "Uleiuri, lichide, filtre și accesorii", PriceMin = 10, PriceMax = 300 },
                new ShopService { Name = "Identificare piesă", Description = "Identificare piesă după VIN și comandă specială", PriceMin = 0, PriceMax = 0 },
            ],
        },
        new()
        {
            Name = "Detailing Studio Cluj Premium",
            Description = "Studio de detailing auto profesional. Servicii complete de îngrijire și protecție pentru pasionații auto care vor perfecțiunea.",
            Address = "Str. Dorobanților nr. 7",
            City = "Cluj-Napoca",
            County = "Cluj",
            Phone = "0745-111222",
            Email = "studio@detailingcluj.ro",
            Website = "https://detailingcluj.ro",
            Category = ShopCategory.Detailing,
            Status = ShopStatus.Active,
            AverageRating = 4.9,
            ReviewCount = 2,
            Hours = WeekdayHours(new TimeOnly(9, 0), new TimeOnly(18, 0), saturdayOpen: true),
            Services =
            [
                new ShopService { Name = "Full Detail exterior", Description = "Decontaminare, clay bar, polish 2 pași, ceară carnauba", PriceMin = 600, PriceMax = 1200 },
                new ShopService { Name = "Full Detail interior", Description = "Curățenie profundă tapițerie, plastic, geamuri", PriceMin = 400, PriceMax = 800 },
                new ShopService { Name = "Paint Protection Film", Description = "Aplicare folie PPF transparentă pe zone vulnerabile", PriceMin = 500, PriceMax = 3000 },
                new ShopService { Name = "Window Tinting", Description = "Foliere geamuri cu folie omologată RAR", PriceMin = 350, PriceMax = 700 },
                new ShopService { Name = "Coating ceramic 9H", Description = "Aplicare coating ceramic profesional cu garanție 5 ani", PriceMin = 1500, PriceMax = 3500 },
            ],
        },
        new()
        {
            Name = "Service Moto Dacia Speed",
            Description = "Service specializat pentru motociclete și scutere. Reparații mecanice, electrică moto și pregătire pentru sezon.",
            Address = "Str. Calea Victoriei nr. 55",
            City = "Pitești",
            County = "Argeș",
            Phone = "0248-234567",
            Email = "contact@moto-speed.ro",
            Category = ShopCategory.ServiceMoto,
            Status = ShopStatus.Active,
            AverageRating = 4.6,
            ReviewCount = 1,
            Hours = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 30), saturdayOpen: true),
            Services =
            [
                new ShopService { Name = "Revizie moto completă", Description = "Schimb ulei, filtre, verificare lanț și frâne", PriceMin = 200, PriceMax = 450 },
                new ShopService { Name = "Reglaj carburator/injecție", Description = "Reglaj ralanti și mapare ECU", PriceMin = 150, PriceMax = 350 },
                new ShopService { Name = "Pregătire sezon", Description = "Verificare completă înainte de sezonul de vară", PriceMin = 300, PriceMax = 500 },
                new ShopService { Name = "Reparație electrică", Description = "Diagnosticare și reparație instalație electrică", PriceMin = 100, PriceMax = 400 },
            ],
        },
    };

    db.Shops.AddRange(shops);
    await db.SaveChangesAsync();
}

// ─── Role Seeding ─────────────────────────────────────────────────────────────
static async Task SeedRolesAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = ["Admin", "ShopOwner", "User"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// ─── Admin Seeding ────────────────────────────────────────────────────────────
static async Task SeedAdminAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string adminEmail = "julian@pitstop.ro";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(admin, "juliandev0011!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

