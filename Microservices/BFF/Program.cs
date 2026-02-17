using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ==============================================
// YARP Reverse Proxy Configuration
// ==============================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ==============================================
// Authentication: JWT from Cookie HttpOnly
// ==============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var cookieSettings = builder.Configuration.GetSection("CookieSettings");
var accessTokenCookieName = cookieSettings["AccessTokenCookieName"] ?? "access_token";
var secretKey = jwtSettings["SecretKey"];

// Only configure JWT auth if settings are provided
if (!string.IsNullOrEmpty(secretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey))
            };
            
            // Extract JWT from HttpOnly Cookie
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Try to get token from cookie first
                    if (context.Request.Cookies.TryGetValue(accessTokenCookieName, out var cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                    // Fallback: Authorization header is handled automatically
                    return Task.CompletedTask;
                }
            };
        });
}
else
{
    // Development mode without JWT validation
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

// ==============================================
// Health Checks for downstream services
// ==============================================
var healthChecksBuilder = builder.Services.AddHealthChecks();

// Add health checks for each downstream service
var clusters = builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren();
foreach (var cluster in clusters)
{
    var address = cluster.GetSection("Destinations:default:Address").Value;
    if (!string.IsNullOrEmpty(address))
    {
        var serviceName = cluster.Key.Replace("-cluster", "");
        healthChecksBuilder.AddUrlGroup(
            new Uri($"{address}/health"),
            name: $"{serviceName}-health",
            tags: new[] { "downstream" });
    }
}

// ==============================================
// CORS Configuration
// ==============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ==============================================
// Logging
// ==============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ==============================================
// Middleware Pipeline
// ==============================================

// CORS must be before routing
app.UseCors("AllowFrontend");

// Health check endpoint
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("downstream")
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// YARP Reverse Proxy - routes all traffic to downstream services
app.MapReverseProxy();

// Fallback for root path
app.MapGet("/", () => Results.Ok(new 
{ 
    service = "CryptoJackpot BFF Gateway",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.Run();

