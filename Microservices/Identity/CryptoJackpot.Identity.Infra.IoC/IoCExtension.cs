using System.Text;
using Asp.Versioning;
using CryptoJackpot.Domain.Core.Behaviors;
using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Application;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.Consumers;
using CryptoJackpot.Identity.Application.Http;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Services;
using CryptoJackpot.Identity.Data;
using CryptoJackpot.Identity.Data.Configuration;
using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Data.Repositories;
using CryptoJackpot.Identity.Data.Services;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Infra.IoC;
using FluentValidation;
using MassTransit;
using MediatR;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CryptoJackpot.Identity.Infra.IoC;

public static class IoCExtension
{
    public static void AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddConfiguration(services, configuration);
        AddAuthentication(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddApplicationServices(services);
        AddInfrastructure(services, configuration);
    }

    /// <summary>
    /// Applies pending database migrations only in development environment.
    /// </summary>
    public async static Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<IdentityDbContext>>();

        try
        {
            var context = services.GetRequiredService<IdentityDbContext>();
            var env = services.GetRequiredService<IHostEnvironment>();

            if (env.IsDevelopment())
            {
                // Check if database exists and can connect
                if (await context.Database.CanConnectAsync())
                {
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    if (pendingMigrations.Count > 0)
                    {
                        logger.LogInformation("Applying {Count} pending migrations for IdentityDbContext...", pendingMigrations.Count);
                        try
                        {
                            await context.Database.MigrateAsync();
                            logger.LogInformation("Migrations applied successfully.");
                        }
                        catch (PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
                        {
                            logger.LogWarning(ex, "Some tables already exist, skipping migration. Consider updating __EFMigrationsHistory table manually.");
                        }
                    }
                }
                else
                {
                    logger.LogInformation("Database does not exist, creating...");
                    await context.Database.MigrateAsync();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            // Don't throw in development - allow app to start even if migrations fail
            if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                throw new InvalidOperationException("Failed to apply migrations for IdentityDbContext.", ex);
            }
        }
    }

    private static void AddConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfig>(configuration.GetSection("JwtSettings"));
        services.Configure<DigitalOceanSettings>(configuration.GetSection("DigitalOcean"));
        services.Configure<KeycloakSettings>(configuration.GetSection("Keycloak"));
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var keycloakSection = configuration.GetSection("Keycloak");
        var useKeycloak = keycloakSection.Exists() && !string.IsNullOrEmpty(keycloakSection["Authority"]);

        if (useKeycloak)
        {
            // Use Keycloak OIDC authentication
            services.AddKeycloakAuthentication(configuration);
        }
        else
        {
            // Fallback to legacy JWT authentication (for backward compatibility)
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
        services.AddDbContextPool<IdentityDbContext>(options =>
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
                Title = "CryptoJackpot Identity API",
                Version = "v1",
                Description = "Identity microservice for authentication and user management"
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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserReferralRepository, UserReferralRepository>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        var assembly = typeof(IAssemblyReference).Assembly;
        
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Validators
        services.AddValidatorsFromAssembly(assembly);
        
        // Validation Behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // AutoMapper
        services.AddAutoMapper(cfg => cfg.AddProfile<IdentityMappingProfile>());

        // Infrastructure Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IIdentityEventPublisher, IdentityEventPublisher>();
        services.AddScoped<IStorageService, DigitalOceanStorageService>();
        
        // Register the DelegatingHandler for Keycloak admin token management
        services.AddTransient<KeycloakAdminTokenHandler>();
        
        // Keycloak User Service (requires admin token)
        services.AddHttpClient<IKeycloakUserService, KeycloakUserService>()
            .AddHttpMessageHandler<KeycloakAdminTokenHandler>();
        
        // Keycloak Role Service (requires admin token)
        services.AddHttpClient<IKeycloakRoleService, KeycloakRoleService>()
            .AddHttpMessageHandler<KeycloakAdminTokenHandler>();
        
        // Keycloak Token Service (does NOT require admin token - uses ROPC flow)
        services.AddHttpClient<IKeycloakTokenService, KeycloakTokenService>();
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Outbox
        DependencyContainer.RegisterServicesWithKafka<IdentityDbContext>(
            services,
            configuration,
            configureRider: rider =>
            {
                // Register producers for events that Identity publishes
                rider.AddProducer<UserRegisteredEvent>(KafkaTopics.UserRegistered);
                rider.AddProducer<PasswordResetRequestedEvent>(KafkaTopics.PasswordResetRequested);
                rider.AddProducer<ReferralCreatedEvent>(KafkaTopics.ReferralCreated);
                rider.AddProducer<UserLoggedInEvent>(KafkaTopics.UserLoggedIn);
                rider.AddProducer<GetUsersForMarketingResponseEvent>(KafkaTopics.GetUsersForMarketingResponse);
                
                // Register consumer for marketing users request (Saga pattern)
                rider.AddConsumer<GetUsersForMarketingConsumer>();
            },
            configureKafkaEndpoints: (context, kafka) =>
            {
                // Marketing users request - Saga pattern for async Request/Response
                kafka.TopicEndpoint<GetUsersForMarketingRequestEvent>(
                    KafkaTopics.GetUsersForMarketingRequest,
                    KafkaTopics.IdentityGroup,
                    e =>
                    {
                        e.ConfigureConsumer<GetUsersForMarketingConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });
            });
    }
}