using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PitStop.Domain.Entities;
using PitStop.Domain.Enums;
using PitStop.Infrastructure.Data;
using PitStop.Infrastructure.Identity;

namespace PitStop.Web.Seeding;

internal static class DataSeeder
{
    internal static async Task SeedAllAsync(IServiceProvider services, bool seedDevData = false)
    {
        await SeedRolesAsync(services);
        await SeedAdminAsync(services);
        await SeedSuperAdminAsync(services);
        if (!seedDevData) return;
        await SeedUsersAsync(services);
        await SeedShopsAsync(services);
        await SeedReviewsAsync(services);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var roleManager       = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "SuperAdmin", "Admin", "ShopOwner", "User" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    private static async Task SeedSuperAdminAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "superadmin@pitstop.ro";
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var superAdmin = new ApplicationUser
        {
            UserName       = email,
            Email          = email,
            FullName       = "Super Admin",
            EmailConfirmed = true,
            CreatedAt      = DateTime.UtcNow,
        };
        await userManager.CreateAsync(superAdmin, "SuperAdmin1234!");
        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
    }

    private static async Task SeedAdminAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "julian@pitstop.ro";
        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            FullName       = "Admin",
            EmailConfirmed = true,
            CreatedAt      = DateTime.UtcNow,
        };
        await userManager.CreateAsync(admin, "juliandev0011!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Maps owner email → shop names they own (one owner can own multiple shops).
    private static readonly (string Email, string ShopName)[] ShopOwnerAssignments =
    [
        ("andrei.popescu@gmail.com",    "Auto Expert Cluj"),
        ("andrei.popescu@gmail.com",    "Service Moto & ATV Cluj"),
        ("maria.ionescu@yahoo.com",     "Detailing Studio Cluj Premium"),
        ("maria.ionescu@yahoo.com",     "Detailing & Protecție Auto București"),
        ("bogdan.constantin@gmail.com", "Vulcanizare Non-Stop Titan"),
        ("bogdan.constantin@gmail.com", "Vulcanizare Rapid Iași"),
        ("elena.dumitrescu@gmail.com",  "Service BMW & Mercedes Sibiu"),
        ("mihai.stanescu@outlook.com",  "Service Auto Renault & Dacia Craiova"),
        ("ioana.georgescu@gmail.com",   "Spălătorie Auto Premium Timișoara"),
        ("ioana.georgescu@gmail.com",   "Spălătorie Eco Auto Oradea"),
    ];

    private static async Task SeedUsersAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var shopOwnerEmails = ShopOwnerAssignments.Select(x => x.Email).ToHashSet();

        var users = new[]
        {
            ("Andrei Popescu",    "andrei.popescu@gmail.com"),
            ("Maria Ionescu",     "maria.ionescu@yahoo.com"),
            ("Bogdan Constantin", "bogdan.constantin@gmail.com"),
            ("Elena Dumitrescu",  "elena.dumitrescu@gmail.com"),
            ("Mihai Stanescu",    "mihai.stanescu@outlook.com"),
            ("Ioana Georgescu",   "ioana.georgescu@gmail.com"),
            ("Alexandru Marin",   "alex.marin@yahoo.com"),
            ("Cristina Florescu", "cristina.florescu@gmail.com"),
            ("Radu Popa",         "radu.popa@gmail.com"),
            ("Ana Stoica",        "ana.stoica@yahoo.com"),
        };

        foreach (var (name, email) in users)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;
            var user = new ApplicationUser
            {
                UserName       = email,
                Email          = email,
                FullName       = name,
                EmailConfirmed = true,
                CreatedAt      = DateTime.UtcNow,
            };
            var result = await userManager.CreateAsync(user, "Parola1234!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, shopOwnerEmails.Contains(email) ? "ShopOwner" : "User");
        }
    }

    private static async Task SeedShopsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db                = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Shops.AnyAsync()) return;

        db.Shops.AddRange(BuildShops());
        await db.SaveChangesAsync();

        foreach (var (email, shopName) in ShopOwnerAssignments)
        {
            var owner = await userManager.FindByEmailAsync(email);
            if (owner is null) continue;
            var shop = await db.Shops.FirstOrDefaultAsync(s => s.Name == shopName);
            if (shop is null) continue;
            shop.OwnerId = owner.Id;
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedReviewsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db                = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Reviews.AnyAsync()) return;

        var seedEmails = new[]
        {
            "andrei.popescu@gmail.com", "maria.ionescu@yahoo.com",    "bogdan.constantin@gmail.com",
            "elena.dumitrescu@gmail.com", "mihai.stanescu@outlook.com", "ioana.georgescu@gmail.com",
            "alex.marin@yahoo.com",     "cristina.florescu@gmail.com", "radu.popa@gmail.com",
            "ana.stoica@yahoo.com",
        };

        var users = new List<(string Id, string Name, string Initials)>();
        foreach (var email in seedEmails)
        {
            var u = await userManager.FindByEmailAsync(email);
            if (u is not null)
                users.Add((u.Id, u.FullName, GetInitials(u.FullName)));
        }
        if (users.Count == 0) return;

        // Two reviews per shop; users rotate so no user reviews the same shop twice.
        var templates = new (int Rating, string Text)[]
        {
            (5, "Serviciu excelent, personal profesionist și prețuri corecte. Recomand cu toată încrederea!"),
            (4, "Calitate bună a lucrărilor, personal prietenos. Voi reveni cu siguranță."),
            (5, "Am fost extrem de mulțumit. Totul a fost gata mai devreme decât promis."),
            (4, "Bun raport calitate-preț. Comunicare clară și transparentă pe tot parcursul."),
            (5, "Lucrare impecabilă. Merită fiecare leu investit, nu am ce reproșa."),
            (4, "Profesioniști serioși. Mic minus pentru că trebuia programare în avans."),
            (5, "Personal amabil și transparent cu privire la costuri. Cel mai bun din zonă."),
            (4, "Rapid și eficient. Am apreciat că m-au ținut la curent cu stadiul lucrării."),
            (5, "Calitate excelentă a serviciilor. Recomand tuturor cunoscuților fără rezerve."),
            (3, "Lucrarea a durat mai mult decât estimat inițial, dar rezultatul a fost corect."),
            (5, "Surprinzător de buni. Nu mă așteptam la un astfel de nivel de profesionalism."),
            (4, "Mulțumit de servicii. Prețul a fost corect față de calitatea livrată."),
        };

        var shops   = await db.Shops.OrderBy(s => s.Id).ToListAsync();
        var reviews = new List<Review>();

        for (int i = 0; i < shops.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var userIdx = (i * 2 + j) % users.Count;
                var tmpl    = templates[(i + j) % templates.Length];
                var user    = users[userIdx];
                reviews.Add(new Review
                {
                    ShopId        = shops[i].Id,
                    UserId        = user.Id,
                    UserName      = user.Name,
                    UserInitials  = user.Initials,
                    Rating        = tmpl.Rating,
                    Text          = tmpl.Text,
                });
            }
        }

        db.Reviews.AddRange(reviews);
        await db.SaveChangesAsync();

        // Recalculate denormalized rating fields on each shop.
        foreach (var shop in shops)
        {
            var stats = await db.Reviews
                .Where(r => r.ShopId == shop.Id)
                .GroupBy(r => r.ShopId)
                .Select(g => new { Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
                .FirstOrDefaultAsync();
            if (stats is null) continue;
            shop.AverageRating = Math.Round(stats.Avg, 1);
            shop.ReviewCount   = stats.Count;
        }
        await db.SaveChangesAsync();
    }

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}" : parts.Length == 1 ? $"{parts[0][0]}" : "?";
    }

    // ── Shop data ────────────────────────────────────────────────────────────────

    private static List<Shop> BuildShops() =>
    [
        new()
        {
            Name          = "Auto Expert Cluj",
            Description   = "Service auto autorizat cu experiență de peste 15 ani. Oferim reparații complete pentru orice marcă de autoturism, diagnosticare computerizată și revizie tehnică.",
            Address       = "Str. Fabricii nr. 42",
            City          = "Cluj-Napoca",
            County        = "Cluj",
            Phone         = "0264-123456",
            Email         = "contact@autoexpertcluj.ro",
            Website       = "https://autoexpertcluj.ro",
            Category      = ShopCategory.ServiceAuto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(18, 0)),
            Services      =
            [
                new() { Name = "Schimb ulei și filtre",       Description = "Schimb ulei motor, filtru ulei, filtru aer, filtru habitaclu", PriceMin = 150, PriceMax = 350  },
                new() { Name = "Diagnosticare computerizată", Description = "Citire erori OBD2, resetare martori bord",                     PriceMin = 80,  PriceMax = 80   },
                new() { Name = "Frâne față/spate",            Description = "Înlocuire plăcuțe și discuri de frână",                       PriceMin = 200, PriceMax = 600  },
                new() { Name = "Distribuție",                 Description = "Înlocuire curea sau lanț de distribuție",                      PriceMin = 500, PriceMax = 1200 },
                new() { Name = "Revizie tehnică completă",    Description = "Revizie periodică conform normelor producătorului",             PriceMin = 300, PriceMax = 700  },
            ],
        },
        new()
        {
            Name          = "Vulcanizare Non-Stop Titan",
            Description   = "Vulcanizare rapidă non-stop în București. Montaj anvelope, echilibrare, geometrie roți și reparații urgente.",
            Address       = "Bd. Iuliu Maniu nr. 15",
            City          = "București",
            County        = "București",
            Phone         = "0721-456789",
            Email         = "titan@vulcanizare.ro",
            Category      = ShopCategory.Vulcanizare,
            Status        = ShopStatus.Active,
            Hours         = AllDayHours(new TimeOnly(0, 0), new TimeOnly(23, 59)),
            Services      =
            [
                new() { Name = "Montaj anvelopă",   Description = "Demontare și montare anvelopă pe jantă",             PriceMin = 20,  PriceMax = 40  },
                new() { Name = "Echilibrare roată", Description = "Echilibrare dinamică cu aparat computerizat",         PriceMin = 15,  PriceMax = 25  },
                new() { Name = "Geometrie roți 3D", Description = "Reglaj geometrie față-spate cu echipament 3D",        PriceMin = 120, PriceMax = 180 },
                new() { Name = "Reparație pană",    Description = "Reparație pană cu spanac sau petic interior",         PriceMin = 30,  PriceMax = 60  },
            ],
        },
        new()
        {
            Name          = "Spălătorie Auto Premium Timișoara",
            Description   = "Spălătorie auto manuală și automată. Curățenie interioară completă, polish și tratamente ceramice pentru protecție de lungă durată.",
            Address       = "Calea Șagului nr. 88",
            City          = "Timișoara",
            County        = "Timiș",
            Phone         = "0256-789012",
            Email         = "office@spalatoriepremium.ro",
            Website       = "https://spalatoriepremium.ro",
            Category      = ShopCategory.Spalatorie,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(7, 30), new TimeOnly(20, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Spălare exterioară manuală",  Description = "Spălare caroserie, jante și geamuri manual",         PriceMin = 50,  PriceMax = 80   },
                new() { Name = "Curățenie interior completă", Description = "Aspirare, șamponare tapițerie și plastic",            PriceMin = 150, PriceMax = 300  },
                new() { Name = "Polish caroserie",            Description = "Înlăturare zgârieturi fine, refacere luciu",          PriceMin = 400, PriceMax = 900  },
                new() { Name = "Tratament ceramic",           Description = "Aplicare coating ceramic pentru protecție 2 ani",     PriceMin = 800, PriceMax = 1500 },
                new() { Name = "Ozon interior",               Description = "Tratament dezinfectare cu ozon a habitaclului",       PriceMin = 80,  PriceMax = 80   },
            ],
        },
        new()
        {
            Name          = "Vopsitorie Auto Brașov Color",
            Description   = "Vopsitorie auto profesională cu cabină de vopsit modernă. Reparații caroserie, tinichigerie și vopsire cu garanție 3 ani.",
            Address       = "Str. Lungă nr. 120",
            City          = "Brașov",
            County        = "Brașov",
            Phone         = "0268-345678",
            Email         = "contact@brasovcolor.ro",
            Category      = ShopCategory.Vopsitorie,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 0), saturdayOpen: false),
            Services      =
            [
                new() { Name = "Vopsire panou",        Description = "Vopsire aripă, ușă sau capotă cu vopsea originală",   PriceMin = 300,  PriceMax = 600  },
                new() { Name = "Reparație zgârietură", Description = "Reparație locală scratch sau lovitură mică",           PriceMin = 100,  PriceMax = 250  },
                new() { Name = "Îndreptare caroserie", Description = "Tinichigerie și îndreptare table cu extractor PDR",    PriceMin = 150,  PriceMax = 500  },
                new() { Name = "Vopsire integrală",    Description = "Revopsire completă autoturism în culoarea dorită",     PriceMin = 2500, PriceMax = 6000 },
            ],
        },
        new()
        {
            Name          = "ITP Rapid Iași",
            Description   = "Stație ITP autorizată RAR. Inspecție tehnică periodică rapidă, fără programare, în maximum 45 minute.",
            Address       = "Str. Nicolina nr. 33",
            City          = "Iași",
            County        = "Iași",
            Phone         = "0232-567890",
            Email         = "itp@rapid-iasi.ro",
            Category      = ShopCategory.ITP,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(7, 0), new TimeOnly(19, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "ITP autoturism",    Description = "Inspecție tehnică periodică autoturism categoria M1", PriceMin = 140, PriceMax = 140 },
                new() { Name = "ITP autoutilitară", Description = "ITP vehicule utilitare până la 3.5t",                 PriceMin = 180, PriceMax = 220 },
                new() { Name = "Verificare emisii", Description = "Control gaze evacuare conform normelor Euro",          PriceMin = 50,  PriceMax = 50  },
            ],
        },
        new()
        {
            Name          = "Piese Auto Dedeman Constanța",
            Description   = "Magazin de piese auto cu stoc larg pentru autoturisme europene și asiatice. Livrare rapidă și consiliere tehnică gratuită.",
            Address       = "Bd. Tomis nr. 210",
            City          = "Constanța",
            County        = "Constanța",
            Phone         = "0241-678901",
            Email         = "vanzari@pieseauto-constanta.ro",
            Website       = "https://pieseauto-constanta.ro",
            Category      = ShopCategory.PieseAuto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 30), new TimeOnly(18, 30), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Piese originale OEM", Description = "Piese originale producător pentru orice marcă",           PriceMin = 50, PriceMax = 2000 },
                new() { Name = "Piese aftermarket",   Description = "Piese aftermarket certificate calitate OE",                PriceMin = 20, PriceMax = 800  },
                new() { Name = "Consumabile auto",    Description = "Uleiuri, lichide, filtre și accesorii",                    PriceMin = 10, PriceMax = 300  },
                new() { Name = "Identificare piesă",  Description = "Identificare piesă după VIN și comandă specială",          PriceMin = 0,  PriceMax = 0    },
            ],
        },
        new()
        {
            Name          = "Detailing Studio Cluj Premium",
            Description   = "Studio de detailing auto profesional. Servicii complete de îngrijire și protecție pentru pasionații auto care vor perfecțiunea.",
            Address       = "Str. Dorobanților nr. 7",
            City          = "Cluj-Napoca",
            County        = "Cluj",
            Phone         = "0745-111222",
            Email         = "studio@detailingcluj.ro",
            Website       = "https://detailingcluj.ro",
            Category      = ShopCategory.Detailing,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(9, 0), new TimeOnly(18, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Full Detail exterior",  Description = "Decontaminare, clay bar, polish 2 pași, ceară carnauba",    PriceMin = 600,  PriceMax = 1200 },
                new() { Name = "Full Detail interior",  Description = "Curățenie profundă tapițerie, plastic, geamuri",            PriceMin = 400,  PriceMax = 800  },
                new() { Name = "Paint Protection Film", Description = "Aplicare folie PPF transparentă pe zone vulnerabile",       PriceMin = 500,  PriceMax = 3000 },
                new() { Name = "Window Tinting",        Description = "Foliere geamuri cu folie omologată RAR",                    PriceMin = 350,  PriceMax = 700  },
                new() { Name = "Coating ceramic 9H",    Description = "Aplicare coating ceramic profesional cu garanție 5 ani",    PriceMin = 1500, PriceMax = 3500 },
            ],
        },
        new()
        {
            Name          = "Service Moto Dacia Speed",
            Description   = "Service specializat pentru motociclete și scutere. Reparații mecanice, electrică moto și pregătire pentru sezon.",
            Address       = "Str. Calea Victoriei nr. 55",
            City          = "Pitești",
            County        = "Argeș",
            Phone         = "0248-234567",
            Email         = "contact@moto-speed.ro",
            Category      = ShopCategory.ServiceMoto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 30), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Revizie moto completă",      Description = "Schimb ulei, filtre, verificare lanț și frâne",  PriceMin = 200, PriceMax = 450 },
                new() { Name = "Reglaj carburator/injecție", Description = "Reglaj ralanti și mapare ECU",                    PriceMin = 150, PriceMax = 350 },
                new() { Name = "Pregătire sezon",            Description = "Verificare completă înainte de sezonul de vară",  PriceMin = 300, PriceMax = 500 },
                new() { Name = "Reparație electrică",        Description = "Diagnosticare și reparație instalație electrică", PriceMin = 100, PriceMax = 400 },
            ],
        },

        // ── New shops ──────────────────────────────────────────────────────────────

        new()
        {
            Name          = "Tuning Auto Garage București",
            Description   = "Atelier specializat în tuning auto: sisteme de evacuare sport, suspensii coborâte, foliaj auto și accesorii premium pentru pasionații auto.",
            Address       = "Str. Biharia nr. 8",
            City          = "București",
            County        = "București",
            Phone         = "0722-333444",
            Email         = "contact@tuninggarage.ro",
            Website       = "https://tuninggarage.ro",
            Category      = ShopCategory.Tuning,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(9, 0), new TimeOnly(18, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Evacuare sport inox", Description = "Sistem evacuare sport inox personalizat pentru orice model",    PriceMin = 800,  PriceMax = 2500 },
                new() { Name = "Suspensie sport",     Description = "Kit suspensie coborâtă cu reglaj înălțime",                    PriceMin = 600,  PriceMax = 2000 },
                new() { Name = "Foliaj auto",         Description = "Foliere integrală sau parțială cu folie premium 3M",           PriceMin = 1500, PriceMax = 5000 },
                new() { Name = "Jante sport aliaj",   Description = "Montaj și echilibrare jante aliaj 17–20 inch",                 PriceMin = 100,  PriceMax = 400  },
            ],
        },
        new()
        {
            Name          = "Service BMW & Mercedes Sibiu",
            Description   = "Service specializat exclusiv pe mărci premium: BMW, Mercedes, Audi, Volkswagen. Tehnicieni certificați, echipamente de ultimă generație.",
            Address       = "Calea Dumbrăvii nr. 105",
            City          = "Sibiu",
            County        = "Sibiu",
            Phone         = "0269-445566",
            Email         = "office@bmwservice-sibiu.ro",
            Website       = "https://bmwservice-sibiu.ro",
            Category      = ShopCategory.ServiceAuto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 0), saturdayOpen: false),
            Brands        = [new() { Name = "BMW" }, new() { Name = "Mercedes" }, new() { Name = "Audi" }, new() { Name = "Volkswagen" }],
            Services      =
            [
                new() { Name = "Diagnosticare BMW/Mercedes", Description = "Diagnosticare completă cu ISTA+ și XENTRY",                       PriceMin = 100, PriceMax = 150 },
                new() { Name = "Service cutie automată",     Description = "Reparație și înlocuire cutie de viteze automată",                  PriceMin = 800, PriceMax = 3000 },
                new() { Name = "Programare cheie",           Description = "Programare chei originale și aftermarket",                         PriceMin = 200, PriceMax = 600 },
                new() { Name = "Actualizare software ECU",   Description = "Update software unitate centrală și sisteme ajutătoare",            PriceMin = 150, PriceMax = 400 },
            ],
        },
        new()
        {
            Name          = "Tractări Auto Non-Stop Ploiești",
            Description   = "Servicii de tractare și depanare auto 24/7 în Ploiești și împrejurimi. Intervenție rapidă, macara și platformă auto disponibile.",
            Address       = "Str. Găgeni nr. 44",
            City          = "Ploiești",
            County        = "Prahova",
            Phone         = "0799-112233",
            Email         = "tractari@nonstop-ploiesti.ro",
            Category      = ShopCategory.Tractari,
            Status        = ShopStatus.Active,
            Hours         = AllDayHours(new TimeOnly(0, 0), new TimeOnly(23, 59)),
            Services      =
            [
                new() { Name = "Tractare cu platformă",    Description = "Transport auto pe platformă pe orice distanță",         PriceMin = 100, PriceMax = 400 },
                new() { Name = "Depanare la fața locului", Description = "Intervenție mecanică urgentă pe loc",                   PriceMin = 80,  PriceMax = 200 },
                new() { Name = "Pornire baterie",          Description = "Pornire auto cu baterie descărcată",                    PriceMin = 50,  PriceMax = 80  },
                new() { Name = "Deschidere mașină",        Description = "Deschidere urgentă fără cheie sau cu cheie blocată",    PriceMin = 80,  PriceMax = 150 },
            ],
        },
        new()
        {
            Name          = "Piese Moto Racing Timișoara",
            Description   = "Magazin specializat în piese și accesorii pentru motociclete sport, enduro și scutere. Stoc larg de mărci premium: Akrapovič, Öhlins, Brembo.",
            Address       = "Bd. Revoluției din 1989 nr. 7",
            City          = "Timișoara",
            County        = "Timiș",
            Phone         = "0722-987654",
            Email         = "shop@piese-moto-timi.ro",
            Website       = "https://piese-moto-timi.ro",
            Category      = ShopCategory.PieseMoto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(9, 0), new TimeOnly(18, 30), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Piese originale moto",     Description = "Piese OEM pentru Honda, Yamaha, Kawasaki, Suzuki, BMW Motorrad", PriceMin = 30,  PriceMax = 2000 },
                new() { Name = "Echipament motociclist",   Description = "Căști, geci, pantaloni și mănuși omologate CE",                  PriceMin = 100, PriceMax = 1500 },
                new() { Name = "Anvelope moto",            Description = "Montaj anvelope sport și touring",                               PriceMin = 50,  PriceMax = 300  },
                new() { Name = "Accesorii sport",          Description = "Evacuări Akrapovič, suspensii Öhlins, discuri Brembo",           PriceMin = 200, PriceMax = 3000 },
            ],
        },
        new()
        {
            Name          = "ITP & Service Rapid Craiova",
            Description   = "Stație ITP autorizată RAR și service auto rapid în Craiova. Inspecție în 30 minute, fără programare în avans.",
            Address       = "Str. Brestei nr. 78",
            City          = "Craiova",
            County        = "Dolj",
            Phone         = "0251-334455",
            Email         = "contact@itp-craiova.ro",
            Category      = ShopCategory.ITP,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(7, 30), new TimeOnly(18, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "ITP autoturism",    Description = "Inspecție periodică M1 în 30 de minute",            PriceMin = 130, PriceMax = 130 },
                new() { Name = "ITP autoutilitară", Description = "Inspecție N1/N2 până la 7.5t",                      PriceMin = 170, PriceMax = 230 },
                new() { Name = "Control poluare",   Description = "Verificare emisii gaze conform norma Euro 5/6",      PriceMin = 50,  PriceMax = 50  },
                new() { Name = "Reglaj frâne",      Description = "Reglaj și verificare sistem de frânare",             PriceMin = 80,  PriceMax = 150 },
            ],
        },
        new()
        {
            Name          = "Vulcanizare Rapid Iași",
            Description   = "Vulcanizare cu program extins în Iași. Montaj și echilibrare rapid, reparații pane urgente, stoc anvelope vară/iarnă.",
            Address       = "Str. Moara de Vânt nr. 12",
            City          = "Iași",
            County        = "Iași",
            Phone         = "0745-223344",
            Email         = "rapid@vulcanizare-iasi.ro",
            Category      = ShopCategory.Vulcanizare,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(7, 0), new TimeOnly(20, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Montaj set anvelope",    Description = "Demontare/montare 4 anvelope cu echilibrare inclusă",   PriceMin = 100, PriceMax = 160 },
                new() { Name = "Echilibrare roată",      Description = "Echilibrare computerizată per bucată",                  PriceMin = 12,  PriceMax = 20  },
                new() { Name = "Reparație pană urgentă", Description = "Reparație pană la fața locului în 15 minute",           PriceMin = 30,  PriceMax = 50  },
                new() { Name = "Geometrie 3D",           Description = "Verificare și reglaj unghiuri roți cu echipament 3D",   PriceMin = 120, PriceMax = 160 },
            ],
        },
        new()
        {
            Name          = "Spălătorie Eco Auto Oradea",
            Description   = "Spălătorie ecologică cu apă reciclată și produse bio. Spălare manuală atentă, detailing și ceară de protecție.",
            Address       = "Str. Matei Corvin nr. 3",
            City          = "Oradea",
            County        = "Bihor",
            Phone         = "0259-667788",
            Email         = "eco@spalatorie-oradea.ro",
            Category      = ShopCategory.Spalatorie,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(19, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Spălare exterioară eco",         Description = "Spălare caroserie cu produse bio, fără fosfați",      PriceMin = 40,  PriceMax = 70  },
                new() { Name = "Aspirare interior",              Description = "Aspirare completă interior + curățare bord",           PriceMin = 30,  PriceMax = 50  },
                new() { Name = "Pachet complet ext. + int.",     Description = "Spălare + aspirare + ceară de protecție",              PriceMin = 100, PriceMax = 160 },
                new() { Name = "Ceară protecție caroserie",      Description = "Aplicare ceară carnauba sau sintetică",                PriceMin = 80,  PriceMax = 150 },
            ],
        },
        new()
        {
            Name          = "Service Auto Renault & Dacia Craiova",
            Description   = "Service autorizat specializat pe Renault și Dacia. Piese originale, diagnosticare cu CAN Clip, revizie și reparații complete.",
            Address       = "Str. Caracal nr. 156",
            City          = "Craiova",
            County        = "Dolj",
            Phone         = "0744-556677",
            Email         = "service@renault-dacia-craiova.ro",
            Category      = ShopCategory.ServiceAuto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 30), saturdayOpen: false),
            Brands        = [new() { Name = "Renault" }, new() { Name = "Dacia" }],
            Services      =
            [
                new() { Name = "Revizie Dacia/Renault",     Description = "Revizie completă conform normelor Groupe Renault",          PriceMin = 200, PriceMax = 450 },
                new() { Name = "Diagnosticare CAN Clip",    Description = "Citire erori cu soft oficial Renault CAN Clip",             PriceMin = 70,  PriceMax = 70  },
                new() { Name = "Distribuție Renault dCi",   Description = "Înlocuire kit distribuție motoare dCi",                    PriceMin = 600, PriceMax = 900 },
                new() { Name = "Climatizare",               Description = "Reîncărcare aer condiționat R134/R1234yf",                  PriceMin = 120, PriceMax = 200 },
            ],
        },
        new()
        {
            Name          = "Detailing & Protecție Auto București",
            Description   = "Studio detailing premium în București. Polishing profesional, coating ceramic nano, PPF și foliere estetică pentru iubitorii de mașini impecabile.",
            Address       = "Str. Preciziei nr. 21",
            City          = "București",
            County        = "București",
            Phone         = "0722-778899",
            Email         = "studio@detailing-buc.ro",
            Website       = "https://detailing-buc.ro",
            Category      = ShopCategory.Detailing,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(9, 0), new TimeOnly(19, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Paint Correction Stage 2",  Description = "Polishing profesional 2 etape, eliminare 90% defecte",     PriceMin = 700,  PriceMax = 1500 },
                new() { Name = "Coating ceramic",           Description = "Coating 9H cu garanție 3 ani, duritate maximă",             PriceMin = 1200, PriceMax = 2800 },
                new() { Name = "PPF full front",            Description = "Folie protecție vopsea față completă",                      PriceMin = 2000, PriceMax = 4000 },
                new() { Name = "Interior detailing",        Description = "Curățenie profundă piele/textil, ozon, parfumare",          PriceMin = 400,  PriceMax = 900  },
            ],
        },
        new()
        {
            Name          = "Service Moto & ATV Cluj",
            Description   = "Service moto și ATV cu experiență de 12 ani. Reparații complete, personalizare și pregătire competiție pentru motociclete și quad-uri.",
            Address       = "Str. Tăietura Turcului nr. 47",
            City          = "Cluj-Napoca",
            County        = "Cluj",
            Phone         = "0745-334455",
            Email         = "service@motoatv-cluj.ro",
            Category      = ShopCategory.ServiceMoto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 30), new TimeOnly(17, 30), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Revizie moto completă", Description = "Schimb ulei, filtre, reglaj supape, lanț și frâne",           PriceMin = 180, PriceMax = 400 },
                new() { Name = "Service ATV/Quad",      Description = "Reparații complete ATV, schimb ulei, filtre, revizie",        PriceMin = 200, PriceMax = 500 },
                new() { Name = "Pregătire circuit",     Description = "Setup moto pentru competiție: geometrie, frâne, suspensii",   PriceMin = 300, PriceMax = 800 },
                new() { Name = "Electrică moto",        Description = "Reparație și upgrade instalație electrică motocicletă",       PriceMin = 100, PriceMax = 350 },
            ],
        },
        new()
        {
            Name          = "Piese Auto Import Iași",
            Description   = "Importator direct piese auto pentru mărci europene și asiatice. Prețuri de import, livrare în 24 ore, garanție 12 luni pe orice piesă.",
            Address       = "Str. Bucium nr. 6",
            City          = "Iași",
            County        = "Iași",
            Phone         = "0745-889900",
            Email         = "comenzi@piese-import-iasi.ro",
            Website       = "https://piese-import-iasi.ro",
            Category      = ShopCategory.PieseAuto,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(8, 0), new TimeOnly(17, 0), saturdayOpen: false),
            Services      =
            [
                new() { Name = "Piese import direct",  Description = "Import direct din Germania, Franța, Japonia — prețuri angrosist", PriceMin = 30, PriceMax = 3000 },
                new() { Name = "Identificare după VIN",Description = "Identificare exactă piesă după serie șasiu",                     PriceMin = 0,  PriceMax = 0    },
                new() { Name = "Piese dezmembrare",    Description = "Piese second-hand verificate din dezmembrări certificate",        PriceMin = 20, PriceMax = 500  },
                new() { Name = "Livrare națională",    Description = "Livrare prin curier în 24–48h pe tot teritoriul României",        PriceMin = 15, PriceMax = 25   },
            ],
        },
        new()
        {
            Name          = "Accesorii & Tuning Brașov",
            Description   = "Magazin complet de accesorii auto și tuning ușor. Covorașe, sisteme audio, camere de parcare, senzori și personalizare estetică.",
            Address       = "Str. Zizinului nr. 99",
            City          = "Brașov",
            County        = "Brașov",
            Phone         = "0268-112233",
            Email         = "shop@accesorii-brasov.ro",
            Category      = ShopCategory.Altele,
            Status        = ShopStatus.Active,
            Hours         = WeekdayHours(new TimeOnly(9, 0), new TimeOnly(19, 0), saturdayOpen: true),
            Services      =
            [
                new() { Name = "Instalare cameră 360°", Description = "Montaj și configurare sistem camere 360° bird-view",            PriceMin = 400, PriceMax = 900  },
                new() { Name = "Sistem audio premium",  Description = "Montaj sistem audio: head unit Android, boxe, amplificator",   PriceMin = 500, PriceMax = 2500 },
                new() { Name = "Senzori parcare",        Description = "Montaj senzori față/spate cu afișaj și sonerie",               PriceMin = 150, PriceMax = 350  },
                new() { Name = "Covorașe premium",       Description = "Covorașe cauciuc sau mochetă la comandă după model",           PriceMin = 80,  PriceMax = 250  },
            ],
        },
    ];

    // ── Hour helpers ─────────────────────────────────────────────────────────────

    private static List<ShopHour> WeekdayHours(TimeOnly open, TimeOnly close, bool saturdayOpen = true) =>
        Enumerable.Range(0, 7).Select(d => new ShopHour
        {
            DayOfWeek = d,
            IsClosed  = d == 6 || (d == 5 && !saturdayOpen),
            OpenTime  = (d == 6 || (d == 5 && !saturdayOpen)) ? (TimeOnly?)null : open,
            CloseTime = (d == 6 || (d == 5 && !saturdayOpen)) ? (TimeOnly?)null : close,
        }).ToList();

    private static List<ShopHour> AllDayHours(TimeOnly open, TimeOnly close) =>
        Enumerable.Range(0, 7).Select(d => new ShopHour
        {
            DayOfWeek = d,
            IsClosed  = false,
            OpenTime  = open,
            CloseTime = close,
        }).ToList();
}
