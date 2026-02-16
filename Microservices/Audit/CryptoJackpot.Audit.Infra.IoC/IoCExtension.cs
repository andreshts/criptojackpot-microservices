using System.Text;
using Asp.Versioning;
using CryptoJackpot.Audit.Application;
using CryptoJackpot.Audit.Application.Consumers;
using CryptoJackpot.Audit.Data.Configuration;
using CryptoJackpot.Audit.Data.Context;
using CryptoJackpot.Audit.Data.Repositories;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Behaviors;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Infra.IoC.Extensions;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CryptoJackpot.Audit.Infra.IoC;

public static class IoCExtension
{
    public static void AddAuditServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddAuthentication(services, configuration);
        AddMongoDb(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddApplicationServices(services);
        AddInfrastructure(services, configuration);
    }

    /// <summary>
    /// Ensures MongoDB indexes are created on startup.
    /// </summary>
    public static async Task EnsureMongoDbIndexesAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<AuditDbContext>>();

        try
        {
            var context = services.GetRequiredService<AuditDbContext>();
            logger.LogInformation("Creating MongoDB indexes for Audit...");
            await context.EnsureIndexesCreatedAsync();
            logger.LogInformation("MongoDB indexes created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating MongoDB indexes.");
            throw new InvalidOperationException("Failed to create MongoDB indexes for Audit.", ex);
        }
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // JWT authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");

        // Cookie settings for extracting token from HTTP-only cookies
        var cookieSettings = configuration.GetSection("CookieSettings");
        var accessTokenCookieName = cookieSettings["AccessTokenCookieName"] ?? "access_token";

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

                // Only accept JWT from HTTP-only cookie (no Authorization header)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(accessTokenCookieName, out var cookieToken) 
                            && !string.IsNullOrEmpty(cookieToken))
                        {
                            context.Token = cookieToken;
                        }
                        else
                        {
                            context.NoResult();
                        }
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void AddMongoDb(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<AuditDbContext>();
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CryptoJackpot Audit API",
                Version = "v1",
                Description = "Audit microservice for logging and tracking system activities"
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
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
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

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Domain Bus
        services.AddTransient<IEventBus, CryptoJackpot.Infra.Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        // MassTransit with Kafka for consuming audit events
        services.AddMassTransit(x =>
        {
            // Register the audit log consumer
            x.AddConsumer<AuditLogEventConsumer>();
            x.AddConsumer<UserLoggedInEventConsumer>();

            // In-memory for internal messaging
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider for consuming audit events
            x.AddRider(rider =>
            {
                rider.AddConsumer<AuditLogEventConsumer>();
                rider.AddConsumer<UserLoggedInEventConsumer>();

                rider.UsingKafka((context, kafka) =>
                {
                    kafka.Host(kafkaHost);
                    kafka.ClientId = "cryptojackpot-audit";

                    // Subscribe to audit log topic
                    kafka.TopicEndpoint<AuditLogEvent>(
                        KafkaTopics.AuditLog,
                        KafkaTopics.AuditGroup,
                        e =>
                        {
                            e.ConfigureConsumer<AuditLogEventConsumer>(context);
                            e.ConfigureTopicDefaults(configuration);
                        });

                    // Subscribe to user login events for auditing
                    kafka.TopicEndpoint<UserLoggedInEvent>(
                        KafkaTopics.UserLoggedIn,
                        KafkaTopics.AuditGroup,
                        e =>
                        {
                            e.ConfigureConsumer<UserLoggedInEventConsumer>(context);
                            e.ConfigureTopicDefaults(configuration);
                        });
                });
            });
        });
    }
}
