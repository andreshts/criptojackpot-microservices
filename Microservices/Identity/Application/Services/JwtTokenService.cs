using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoJackpot.Identity.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfig _jwtSettings;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenService(IOptions<JwtConfig> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value 
            ?? throw new InvalidOperationException("JwtSettings are not configured.");

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30) // Allow minimal clock drift
        };
    }

    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Standard claims
            new(JwtRegisteredClaimNames.Sub, user.UserGuid.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Email, user.Email),

            // Custom claims
            new("user_id", user.Id.ToString()),
            new("name", $"{user.Name} {user.LastName}"),
            new("role", user.Role.Name),
            new(ClaimTypes.Role, user.Role.Name),
            new("email_verified", user.EmailVerified.ToString().ToLowerInvariant()),
        };

        // Add 2FA status if enabled
        if (user.TwoFactorEnabled)
        {
            claims.Add(new Claim("2fa_enabled", "true"));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateTwoFactorChallengeToken(User user, int expiresInMinutes = 5)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Minimal claims - NO role, NO permissions
        // Only what's needed to identify the user for 2FA verification
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserGuid.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("purpose", "2fa_challenge"), // Restricted scope - NOT a full access token
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateTwoFactorChallengeToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal is null)
            return null;

        // Verify this is a 2FA challenge token, not a regular access token
        var purposeClaim = principal.FindFirst("purpose");
        if (purposeClaim?.Value != "2fa_challenge")
            return null;

        return GetUserGuidFromClaims(principal);
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Guid? GetUserGuidFromClaims(ClaimsPrincipal principal)
    {
        var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) 
                       ?? principal.FindFirst(ClaimTypes.NameIdentifier);

        if (subClaim != null && Guid.TryParse(subClaim.Value, out var userGuid))
        {
            return userGuid;
        }

        return null;
    }
}

