using CryptoJackpot.Infra.IoC.Extensions;
using CryptoJackpot.Order.Data.Context;
using CryptoJackpot.Order.Data.Extensions;
using CryptoJackpot.Order.Infra.IoC;

var builder = WebApplication.CreateBuilder(args);

// Single point of DI configuration
builder.Services.AddOrderServices(builder.Configuration);

// Health Checks for Kubernetes probes
builder.Services.AddHealthChecks();

var app = builder.Build();

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

// Apply migrations in development
await app.ApplyMigrationsAsync<OrderDbContext>();

await app.ProvisionQuartzSchemaAsync<OrderDbContext>();

await app.RunAsync();
