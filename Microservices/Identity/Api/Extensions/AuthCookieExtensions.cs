using CryptoJackpot.Identity.Application.Configuration;

namespace CryptoJackpot.Identity.Api.Extensions;

/// <summary>
/// Extension methods for setting HttpOnly authentication cookies.
/// This is infrastructure code that belongs in the API layer.
/// </summary>
public static class AuthCookieExtensions
{
    private const int DefaultRefreshTokenDays = 7;
    private const int RememberMeRefreshTokenDays = 30;

    /// <summary>
    /// Sets the access token as an HttpOnly cookie.
    /// </summary>
    public static void SetAccessTokenCookie(
        this HttpResponse response, 
        string token, 
        CookieConfig config,
        int expiresInMinutes)
    {
        var options = CreateCookieOptions(config, TimeSpan.FromMinutes(expiresInMinutes));
        response.Cookies.Append(config.AccessTokenCookieName, token, options);
    }

    /// <summary>
    /// Sets the refresh token as an HttpOnly cookie.
    /// </summary>
    public static void SetRefreshTokenCookie(
        this HttpResponse response, 
        string token, 
        CookieConfig config,
        bool rememberMe = false)
    {
        var days = rememberMe ? RememberMeRefreshTokenDays : DefaultRefreshTokenDays;
        var options = CreateCookieOptions(config, TimeSpan.FromDays(days));
        response.Cookies.Append(config.RefreshTokenCookieName, token, options);
    }

    /// <summary>
    /// Sets both access and refresh token cookies.
    /// </summary>
    public static void SetAuthCookies(
        this HttpResponse response,
        string accessToken,
        string refreshToken,
        CookieConfig config,
        int accessExpiresMinutes,
        bool rememberMe = false)
    {
        response.SetAccessTokenCookie(accessToken, config, accessExpiresMinutes);
        response.SetRefreshTokenCookie(refreshToken, config, rememberMe);
    }

    /// <summary>
    /// Clears both authentication cookies (for logout).
    /// </summary>
    public static void ClearAuthCookies(this HttpResponse response, CookieConfig config)
    {
        var expiredOptions = CreateCookieOptions(config, TimeSpan.Zero);
        expiredOptions.Expires = DateTimeOffset.UtcNow.AddDays(-1);
        
        response.Cookies.Delete(config.AccessTokenCookieName, expiredOptions);
        response.Cookies.Delete(config.RefreshTokenCookieName, expiredOptions);
    }

    /// <summary>
    /// Gets the refresh token from request cookies.
    /// </summary>
    public static string? GetRefreshToken(this HttpRequest request, CookieConfig config)
    {
        return request.Cookies.TryGetValue(config.RefreshTokenCookieName, out var token) ? token : null;
    }

    /// <summary>
    /// Gets the access token from request cookies.
    /// </summary>
    public static string? GetAccessToken(this HttpRequest request, CookieConfig config)
    {
        return request.Cookies.TryGetValue(config.AccessTokenCookieName, out var token) ? token : null;
    }

    private static CookieOptions CreateCookieOptions(CookieConfig config, TimeSpan expiration)
    {
        var sameSite = config.SameSite.ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "lax" => SameSiteMode.Lax,
            _ => SameSiteMode.Strict
        };

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = config.SecureOnly,
            SameSite = sameSite,
            Path = config.Path,
            Expires = DateTimeOffset.UtcNow.Add(expiration),
            IsEssential = true
        };

        if (!string.IsNullOrEmpty(config.Domain))
        {
            options.Domain = config.Domain;
        }

        return options;
    }
}

