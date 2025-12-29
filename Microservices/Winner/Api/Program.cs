using CryptoJackpot.Winner.Application;

var builder = WebApplication.CreateBuilder(args);

// Single point of DI configuration
builder.Services.AddWinnerServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
