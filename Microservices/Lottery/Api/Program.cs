using CryptoJackpot.Domain.Core.Middleware;
using CryptoJackpot.Lottery.Api.Hubs;
using CryptoJackpot.Lottery.Api.Services;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Infra.IoC;

var builder = WebApplication.CreateBuilder(args);

// Single point of DI configuration
builder.Services.AddLotteryServices(builder.Configuration);

// SignalR for real-time lottery updates
builder.Services.AddLotterySignalR(builder.Configuration);

// Register notification service (needs to be in API layer for Hub access)
builder.Services.AddScoped<ILotteryNotificationService, LotteryNotificationService>();

// Health Checks for Kubernetes probes
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handling - must be first in pipeline
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint for Kubernetes liveness/readiness probes
app.MapHealthChecks("/health");
app.MapControllers();

// SignalR hub endpoint
app.MapHub<LotteryHub>("/hubs/lottery");

// Apply migrations in development
await app.ApplyMigrationsAsync();

await app.RunAsync();
