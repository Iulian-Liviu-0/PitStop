using PitStop.Web.Components;
using PitStop.Web.Endpoints;
using PitStop.Web.Extensions;
using PitStop.Web.Seeding;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddIdentityServices();
    builder.Services.AddCookieAuth();
    builder.Services.AddGoogleAuth(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddRepositories();

    var app = builder.Build();

    await app.InitializeDatabaseAsync();

    if (app.Configuration.GetValue<bool>("ResetForRelease"))
        await DataCleaner.ClearAllAsync(app.Services);

    if (app.Configuration.GetValue<bool>("SeedData"))
        await DataSeeder.SeedAllAsync(app.Services, true);

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", true);
        app.UseHsts();
    }

    app.UseSerilogRequestLogging(opts =>
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms");

    app.UseStatusCodePagesWithReExecute("/not-found");
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    app.MapAuthEndpoints();
    app.MapSitemapEndpoint();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}