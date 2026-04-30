using PitStop.Application.Interfaces;
using PitStop.Domain.Enums;
using System.Text;

namespace PitStop.Web.Endpoints;

internal static class SitemapEndpoint
{
    internal static IEndpointRouteBuilder MapSitemapEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sitemap.xml", HandleAsync);
        return app;
    }

    private static async Task HandleAsync(HttpContext ctx, IShopRepository shopRepo)
    {
        var shops   = await shopRepo.GetAllAsync();
        var baseUri = $"{ctx.Request.Scheme}://{ctx.Request.Host}";

        var staticUrls = new[]
        {
            (Url: "/",           Priority: "1.0", Freq: "weekly"),
            (Url: "/servicii",   Priority: "0.9", Freq: "daily"),
            (Url: "/despre-noi", Priority: "0.5", Freq: "monthly"),
            (Url: "/contact",    Priority: "0.5", Freq: "monthly"),
        };

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var (url, priority, freq) in staticUrls)
            sb.AppendLine($"  <url><loc>{baseUri}{url}</loc><changefreq>{freq}</changefreq><priority>{priority}</priority></url>");

        foreach (var shop in shops.Where(s => s.Status == ShopStatus.Active))
        {
            var updated = shop.UpdatedAt.ToString("yyyy-MM-dd");
            sb.AppendLine($"  <url><loc>{baseUri}/serviciu/{shop.Id}</loc><lastmod>{updated}</lastmod><changefreq>weekly</changefreq><priority>0.8</priority></url>");
        }

        sb.AppendLine("</urlset>");
        ctx.Response.ContentType = "application/xml; charset=utf-8";
        await ctx.Response.WriteAsync(sb.ToString());
    }
}
