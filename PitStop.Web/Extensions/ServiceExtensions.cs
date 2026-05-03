using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PitStop.Application.Interfaces;
using PitStop.Infrastructure.Data;
using PitStop.Infrastructure.Identity;
using PitStop.Infrastructure.Repositories;
using PitStop.Infrastructure.Services;
using PitStop.Infrastructure.Storage;

namespace PitStop.Web.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "Postgres";

        services.AddDbContextFactory<AppDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(configuration.GetConnectionString("SqliteConnection") ??
                                  "Data Source=pitstop_dev.db");
            else
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }

    internal static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    internal static IServiceCollection AddCookieAuth(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/auth/login";
            options.AccessDeniedPath = "/auth/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });

        return services;
    }

    internal static IServiceCollection AddGoogleAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var clientSecret = configuration["Authentication:Google:ClientSecret"];

        if (!string.IsNullOrWhiteSpace(clientId) && clientId != "YOUR_GOOGLE_CLIENT_ID")
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret!;
                options.CallbackPath = "/auth/google-callback";
            });

        return services;
    }

    internal static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ShopOwner", policy => policy.RequireRole("ShopOwner"));
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
        });

        return services;
    }

    internal static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IShopRepository, ShopRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IShopRequestRepository, ShopRequestRepository>();
        services.AddScoped<IFavoriteShopRepository, FavoriteShopRepository>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }

    internal static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        var provider = app.Configuration.GetValue<string>("DatabaseProvider") ?? "Postgres";
        if (!provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase)) return;

        await using var scope = app.Services.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }
}