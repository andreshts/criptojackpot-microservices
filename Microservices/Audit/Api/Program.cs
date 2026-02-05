using CryptoJackpot.Audit.Infra.IoC;

var builder = WebApplication.CreateBuilder(args);

// Single point of DI configuration
builder.Services.AddAuditServices(builder.Configuration);

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

// Ensure MongoDB indexes are created
await app.EnsureMongoDbIndexesAsync();

await app.RunAsync();
