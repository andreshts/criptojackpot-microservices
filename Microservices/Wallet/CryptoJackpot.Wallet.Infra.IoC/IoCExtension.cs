using System.Text;
using Asp.Versioning;
using CryptoJackpot.Domain.Core.Behaviors;
using CryptoJackpot.Infra.IoC;
using CryptoJackpot.Wallet.Application;
using CryptoJackpot.Wallet.Application.Providers;
using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Constants;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Polly;
using Polly.Extensions.Http;

namespace CryptoJackpot.Wallet.Infra.IoC;

public static class IoCExtension
{
    public static void AddWalletServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddAuthentication(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddApplicationServices(services);
        AddCoinPayments(services, configuration);
        AddInfrastructure(services, configuration);
    }

    /// <summary>
    /// Applies pending database migrations only in development environment.
    /// </summary>
    public async static Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WalletDbContext>>();

        try
        {
            var context = services.GetRequiredService<WalletDbContext>();
            var env = services.GetRequiredService<IHostEnvironment>();

            if (env.IsDevelopment())
            {
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Count > 0)
                {
                    logger.LogInformation("Applying {Count} pending migrations for WalletDbContext...", pendingMigrations.Count);
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw new InvalidOperationException("Failed to apply migrations for WalletDbContext in development.", ex);
        }
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured");

        // Configure Npgsql DataSource
        // When using PgBouncer in transaction mode, Npgsql's internal pooling works alongside it
        // PgBouncer handles the real connection pool to PostgreSQL (DEFAULT_POOL_SIZE=20)
        // Npgsql manages virtual connections from the application side
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Use AddDbContextPool to reuse DbContext instances (memory optimization)
        // This reduces object creation overhead in high-concurrency scenarios
        // The poolSize here is for DbContext instances, not database connections
        services.AddDbContextPool<WalletDbContext>(options =>
            options.UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention(),
            poolSize: 100);
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CryptoJackpot Wallet API",
                Version = "v1",
                Description = "Wallet microservice for wallet management"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token in format: {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void AddControllers(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                if (allowedOrigins.Length > 0)
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    // For development without specific origins - need SetIsOriginAllowed for credentials
                    // Note: AllowAnyOrigin() cannot be used with AllowCredentials()
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        // Add repositories here
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        var assembly = typeof(IAssemblyReference).Assembly;

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Validators
        services.AddValidatorsFromAssembly(assembly);
        
        // Behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // AutoMapper
        services.AddAutoMapper(assembly);
    }

    private static void AddCoinPayments(IServiceCollection services, IConfiguration configuration)
    {
        var coinPaymentsSettings = configuration.GetSection(ConfigurationKeys.CoinPaymentsSection);
        var privateKey = coinPaymentsSettings["PrivateKey"] 
            ?? throw new InvalidOperationException("CoinPayments PrivateKey is not configured");
        var publicKey = coinPaymentsSettings["PublicKey"] 
            ?? throw new InvalidOperationException("CoinPayments PublicKey is not configured");
        var baseUrl = coinPaymentsSettings["BaseUrl"] ?? ServiceDefaults.CoinPaymentsBaseUrl;
        
        // Configure HttpClient with retry and circuit breaker policies
        services.AddHttpClient(ServiceDefaults.CoinPaymentsHttpClient, client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(ServiceDefaults.HttpClientTimeoutSeconds);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Register the provider
        services.AddSingleton<ICoinPaymentProvider>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new CoinPaymentProvider(privateKey, publicKey, httpClientFactory);
        });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(ResilienceSettings.RetryCount, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                ResilienceSettings.CircuitBreakerFailureThreshold, 
                TimeSpan.FromSeconds(ResilienceSettings.CircuitBreakerDurationSeconds));
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Outbox
        DependencyContainer.RegisterServicesWithKafka<WalletDbContext>(
            services,
            configuration,
            configureRider: _ =>
            {
                // Register producers/consumers for events here
            },
            configureBus: null,
            configureKafkaEndpoints: null,
            useMessageScheduler: false);
    }
}