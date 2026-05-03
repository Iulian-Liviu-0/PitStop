using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using PitStop.Application.Interfaces;
using PitStop.Infrastructure.Identity;

namespace PitStop.Web.Endpoints;

internal static class AuthEndpoints
{
    internal static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/do-login", DoLogin);
        app.MapPost("/auth/do-register", DoRegister);
        app.MapPost("/auth/do-logout", DoLogout);
        app.MapPost("/auth/do-forgot-password", DoForgotPassword);
        app.MapPost("/auth/do-set-password", DoSetPassword);
        app.MapGet("/auth/google-login", GoogleLogin);
        app.MapGet("/auth/google-callback", GoogleCallback);

        return app;
    }

    private static async Task<IResult> DoLogin(HttpContext ctx, SignInManager<ApplicationUser> signInMgr)
    {
        var form = await ctx.Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var rememberMe = form["rememberMe"].ToString() == "true";
        var returnUrl = form["returnUrl"].ToString();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Results.Redirect("/auth/login?error=empty");

        var result = await signInMgr.PasswordSignInAsync(email, password, rememberMe, false);
        if (!result.Succeeded)
            return Results.Redirect("/auth/login?error=invalid");

        var destination = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/') ? returnUrl : "/";
        return Results.Redirect(destination);
    }

    private static async Task<IResult> DoRegister(HttpContext ctx, UserManager<ApplicationUser> userMgr,
        SignInManager<ApplicationUser> signInMgr)
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

        var user = new ApplicationUser
            { UserName = email, Email = email, FullName = name, CreatedAt = DateTime.UtcNow };
        var result = await userMgr.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var code = result.Errors.Any(e => e.Code.Contains("Duplicate")) ? "exists" : "weak";
            return Results.Redirect($"/auth/login?tab=register&regError={code}");
        }

        await userMgr.AddToRoleAsync(user, "User");
        await signInMgr.SignInAsync(user, false);
        return Results.Redirect("/");
    }

    private static async Task<IResult> DoLogout(SignInManager<ApplicationUser> signInMgr)
    {
        await signInMgr.SignOutAsync();
        return Results.Redirect("/");
    }

    private static async Task<IResult> DoForgotPassword(HttpContext ctx, UserManager<ApplicationUser> userMgr,
        IEmailService emailSvc)
    {
        var form = await ctx.Request.ReadFormAsync();
        var email = form["email"].ToString().Trim();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await userMgr.FindByEmailAsync(email);
            if (user is not null)
            {
                var token = await userMgr.GeneratePasswordResetTokenAsync(user);
                var baseUri = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
                var resetUrl =
                    $"{baseUri}/auth/set-password?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
                var html = $"""
                            <p>Bună, {user.FullName}!</p>
                            <p>Am primit o cerere de resetare a parolei pentru contul tău PitStop.</p>
                            <p style="margin:24px 0;">
                              <a href="{resetUrl}" style="background:#C0392B;color:#fff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:bold;">
                                Resetează parola
                              </a>
                            </p>
                            <p style="color:#888;font-size:12px;">Dacă nu ai solicitat resetarea parolei, ignoră acest email. Link-ul expiră în 24 de ore.</p>
                            """;
                try
                {
                    await emailSvc.SendAsync(email, "Resetare parolă PitStop", html);
                }
                catch
                {
                    /* swallow — don't reveal failures */
                }
            }
        }

        // Always redirect to success (never reveal whether the email exists)
        return Results.Redirect("/auth/forgot-password?sent=1");
    }

    private static async Task<IResult> DoSetPassword(HttpContext ctx, UserManager<ApplicationUser> userMgr,
        SignInManager<ApplicationUser> signInMgr)
    {
        var form = await ctx.Request.ReadFormAsync();
        var userId = form["userId"].ToString();
        var token = form["token"].ToString();
        var password = form["password"].ToString();
        var confirmPassword = form["confirmPassword"].ToString();

        if (password != confirmPassword)
            return Results.Redirect(
                $"/auth/set-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}&err=mismatch");

        var user = await userMgr.FindByIdAsync(userId);
        if (user is null)
            return Results.Redirect(
                $"/auth/set-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}&err=invalid");

        var result = await userMgr.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
        {
            var errCode = result.Errors.Any(e => e.Code.Contains("Password")) ? "weak" : "invalid";
            return Results.Redirect(
                $"/auth/set-password?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}&err={errCode}");
        }

        await signInMgr.SignInAsync(user, false);
        var isAdmin = await userMgr.IsInRoleAsync(user, "Admin");
        var isShopOwner = await userMgr.IsInRoleAsync(user, "ShopOwner");
        var destination = isAdmin ? "/admin" : isShopOwner ? "/shop/dashboard" : "/dashboard";
        return Results.Redirect(destination);
    }

    private static IResult GoogleLogin()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = "/auth/google-callback"
        };
        return Results.Challenge(props, ["Google"]);
    }

    private static async Task<IResult> GoogleCallback(HttpContext ctx, UserManager<ApplicationUser> userMgr,
        SignInManager<ApplicationUser> signInMgr)
    {
        var info = await signInMgr.GetExternalLoginInfoAsync();
        if (info is null)
            return Results.Redirect("/auth/login?error=invalid");

        var signInResult = await signInMgr.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        if (signInResult.Succeeded)
            return Results.Redirect("/");

        var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(ClaimTypes.Name)?.Value ?? email ?? "Utilizator";
        if (string.IsNullOrWhiteSpace(email))
            return Results.Redirect("/auth/login?error=invalid");

        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email, Email = email, FullName = name, EmailConfirmed = true, CreatedAt = DateTime.UtcNow
            };
            var createResult = await userMgr.CreateAsync(user);
            if (!createResult.Succeeded)
                return Results.Redirect("/auth/login?error=invalid");
            await userMgr.AddToRoleAsync(user, "User");
        }

        await userMgr.AddLoginAsync(user, info);
        await signInMgr.SignInAsync(user, false);
        return Results.Redirect("/");
    }
}