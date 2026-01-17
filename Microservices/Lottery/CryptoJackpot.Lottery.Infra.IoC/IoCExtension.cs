using System.Text;
using Asp.Versioning;
using CryptoJackpot.Domain.Core.Behaviors;
using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Infra.IoC;
using CryptoJackpot.Lottery.Application;
using CryptoJackpot.Lottery.Application.Configuration;
using CryptoJackpot.Lottery.Application.Consumers;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Application.Services;
using CryptoJackpot.Lottery.Data.Context;
using CryptoJackpot.Lottery.Data.Repositories;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentValidation;
using MassTransit;
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

namespace CryptoJackpot.Lottery.Infra.IoC;

public static class IoCExtension
{
    
   public static void AddLotteryServices(this IServiceCollection services,
       IConfiguration configuration)
    {
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
        var logger = services.GetRequiredService<ILogger<LotteryDbContext>>();

        try
        {
            var context = services.GetRequiredService<LotteryDbContext>();
            var env = services.GetRequiredService<IHostEnvironment>();

            if (env.IsDevelopment())
            {
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Count > 0)
                {
                    logger.LogInformation("Applying {Count} pending migrations for LotteryDbContext...", pendingMigrations.Count);
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw new InvalidOperationException("Failed to apply migrations for LotteryDbContext in development.", ex);
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
                
                // SignalR sends the access token in the query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/lottery"))
                        {
                            context.Token = accessToken;
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<LotteryDbContext>(options =>
            options.UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention());
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CryptoJackpot Lottery API",
                Version = "v1",
                Description = "Lottery microservice for lottery management"
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
                    // For development without specific origins - need SetIsOriginAllowed for SignalR with credentials
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
        services.AddScoped<ILotteryDrawRepository, LotteryDrawRepository>();
        services.AddScoped<IPrizeRepository, PrizeRepository>();
        services.AddScoped<ILotteryNumberRepository, LotteryNumberRepository>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        var assembly = typeof(IAssemblyReference).Assembly;

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Validators
        services.AddValidatorsFromAssembly(assembly);

        //Behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // AutoMapper
        services.AddAutoMapper(typeof(LotteryMappingProfile).Assembly);

        // Application Services
        services.AddScoped<ILotteryNumberService, LotteryNumberService>();
    }

    /// <summary>
    /// Adds SignalR services for real-time lottery updates.
    /// Call this method in the API project's Program.cs.
    /// </summary>
    public static void AddLotterySignalR(this IServiceCollection services)
    {
        services.AddSignalR();
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Outbox
        DependencyContainer.RegisterServicesWithKafka<LotteryDbContext>(
            services,
            configuration,
            configureRider: rider =>
            {
                // Register producer for publishing events
                rider.AddProducer<LotteryCreatedEvent>(KafkaTopics.LotteryCreated);
                rider.AddProducer<NumbersReservedEvent>(KafkaTopics.NumbersReserved);
                
                // Register consumers for internal events
                rider.AddConsumer<LotteryCreatedConsumer>();
                
                // Register consumers for Order events
                rider.AddConsumer<OrderCreatedConsumer>();
                rider.AddConsumer<OrderCompletedConsumer>();
                rider.AddConsumer<OrderExpiredConsumer>();
                rider.AddConsumer<OrderCancelledConsumer>();
            },
            configureKafkaEndpoints: (context, kafka) =>
            {
                // Lottery internal events
                kafka.TopicEndpoint<LotteryCreatedEvent>(
                    KafkaTopics.LotteryCreated,
                    KafkaTopics.LotteryGroup,
                    e =>
                    {
                        e.ConfigureConsumer<LotteryCreatedConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });

                // Order events - reserve numbers when order is created
                kafka.TopicEndpoint<OrderCreatedEvent>(
                    KafkaTopics.OrderCreated,
                    KafkaTopics.LotteryGroup,
                    e =>
                    {
                        e.ConfigureConsumer<OrderCreatedConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });

                // Order events - confirm sold when order is completed
                kafka.TopicEndpoint<OrderCompletedEvent>(
                    KafkaTopics.OrderCompleted,
                    KafkaTopics.LotteryGroup,
                    e =>
                    {
                        e.ConfigureConsumer<OrderCompletedConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });

                // Order events - release numbers when order expires
                kafka.TopicEndpoint<OrderExpiredEvent>(
                    KafkaTopics.OrderExpired,
                    KafkaTopics.LotteryGroup,
                    e =>
                    {
                        e.ConfigureConsumer<OrderExpiredConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });

                // Order events - release numbers when order is cancelled
                kafka.TopicEndpoint<OrderCancelledEvent>(
                    KafkaTopics.OrderCancelled,
                    KafkaTopics.LotteryGroup,
                    e =>
                    {
                        e.ConfigureConsumer<OrderCancelledConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });
            });
    }
}