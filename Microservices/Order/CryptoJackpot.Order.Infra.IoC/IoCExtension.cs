using System.Text;
using Asp.Versioning;
using CryptoJackpot.Domain.Core.Behaviors;
using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Infra.IoC;
using CryptoJackpot.Order.Application;
using CryptoJackpot.Order.Application.Configuration;
using CryptoJackpot.Order.Application.Consumers;
using CryptoJackpot.Order.Application.Interfaces;
using CryptoJackpot.Order.Application.Services;
using CryptoJackpot.Order.Data.Context;
using CryptoJackpot.Order.Data.Repositories;
using CryptoJackpot.Order.Domain.Interfaces;
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
using Quartz;

namespace CryptoJackpot.Order.Infra.IoC;

public static class IoCExtension
{
    public static void AddOrderServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddAuthentication(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddApplicationServices(services);
        AddQuartzScheduler(services, configuration);
        AddInfrastructure(services, configuration);
    }

    /// <summary>
    /// Applies pending database migrations only in development environment.
    /// </summary>
    public async static Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<OrderDbContext>>();

        try
        {
            var context = services.GetRequiredService<OrderDbContext>();
            var env = services.GetRequiredService<IHostEnvironment>();

            if (env.IsDevelopment())
            {
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Count > 0)
                {
                    logger.LogInformation("Applying {Count} pending migrations for OrderDbContext...", pendingMigrations.Count);
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw new InvalidOperationException("Failed to apply migrations for OrderDbContext in development.", ex);
        }
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
            // Fallback to legacy JWT authentication
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
        services.AddDbContextPool<OrderDbContext>(options =>
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
                Title = "CryptoJackpot Order API",
                Version = "v1",
                Description = "Order microservice for order management"
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
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
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
        services.AddAutoMapper(typeof(OrderMappingProfile).Assembly);
    }

    private static void AddQuartzScheduler(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Configure Quartz.NET with PostgreSQL persistence
        services.AddQuartz(q =>
        {

            // Configure persistent job store with PostgreSQL
            q.UsePersistentStore(store =>
            {
                store.UsePostgres(connectionString!);
                store.UseNewtonsoftJsonSerializer();
                
                // Use clustered mode for multi-instance support
                store.UseClustering(c =>
                {
                    c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                    c.CheckinInterval = TimeSpan.FromSeconds(10);
                });

                // Configure table prefix (optional, for organization)
                store.SetProperty("quartz.jobStore.tablePrefix", "qrtz_");
            });

            // Scheduler settings
            q.SchedulerName = "OrderTimeoutScheduler";
            q.SchedulerId = "AUTO";

            // Configure misfire handling - fire immediately if trigger was missed
            q.MisfireThreshold = TimeSpan.FromSeconds(60);
        });

        // Add Quartz hosted service
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
            options.AwaitApplicationStarted = true;
        });

        // Register the order timeout scheduler service
        services.AddScoped<IOrderTimeoutScheduler, QuartzOrderTimeoutScheduler>();

        // Register the background cleanup service as a fallback
        services.AddHostedService<ExpiredOrdersCleanupService>();
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Outbox
        // Note: We use Quartz.NET for order timeout scheduling instead of MassTransit scheduler
        DependencyContainer.RegisterServicesWithKafka<OrderDbContext>(
            services,
            configuration,
            configureRider: rider =>
            {
                // Register producers for Order events
                rider.AddProducer<OrderCreatedEvent>(KafkaTopics.OrderCreated);
                rider.AddProducer<OrderCompletedEvent>(KafkaTopics.OrderCompleted);
                rider.AddProducer<OrderExpiredEvent>(KafkaTopics.OrderExpired);
                rider.AddProducer<OrderCancelledEvent>(KafkaTopics.OrderCancelled);

                // Register consumer for NumbersReserved events from Lottery
                rider.AddConsumer<NumbersReservedConsumer>();
            },
            configureKafkaEndpoints: (context, kafka) =>
            {
                // NumbersReserved - create/update orders when numbers are reserved via SignalR
                kafka.TopicEndpoint<NumbersReservedEvent>(
                    KafkaTopics.NumbersReserved,
                    KafkaTopics.OrderGroup,
                    e =>
                    {
                        e.ConfigureConsumer<NumbersReservedConsumer>(context);
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                    });
            });
    }
}