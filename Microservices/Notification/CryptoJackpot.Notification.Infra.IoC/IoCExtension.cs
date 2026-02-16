using Asp.Versioning;
using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Notification;
using CryptoJackpot.Infra.IoC;
using CryptoJackpot.Infra.IoC.Extensions;
using CryptoJackpot.Notification.Application;
using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Consumers;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Application.Providers;
using CryptoJackpot.Notification.Data.Context;
using CryptoJackpot.Notification.Data.Repositories;
using CryptoJackpot.Notification.Domain.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Npgsql;
namespace CryptoJackpot.Notification.Infra.IoC;

public static class IoCExtension
{
    public static void AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddConfiguration(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddProviders(services);
        AddApplicationServices(services);
        AddInfrastructure(services, configuration);
    }

    /// <summary>
    /// Applies pending database migrations only in development environment.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<NotificationDbContext>>();

        try
        {
            var context = services.GetRequiredService<NotificationDbContext>();
            var env = services.GetRequiredService<IHostEnvironment>();

            if (env.IsDevelopment())
            {
                // Check if database exists and can connect
                if (await context.Database.CanConnectAsync())
                {
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    if (pendingMigrations.Count > 0)
                    {
                        logger.LogInformation("Applying {Count} pending migrations for NotificationDbContext...", pendingMigrations.Count);
                        try
                        {
                            await context.Database.MigrateAsync();
                            logger.LogInformation("Migrations applied successfully.");
                        }
                        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
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
                throw new InvalidOperationException("Failed to apply migrations for NotificationDbContext.", ex);
            }
        }
    }

    private static void AddConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NotificationConfiguration>(configuration);
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured");

        // Configure Npgsql DataSource
        // When using PgBouncer in transaction mode, Npgsql's internal pooling works alongside it
        // PgBouncer handles the real connection pool to PostgreSQL (DEFAULT_POOL_SIZE=20)
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Use AddDbContextPool to reuse DbContext instances (memory optimization)
        services.AddDbContextPool<NotificationDbContext>(options =>
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
                Title = "CryptoJackpot Notification API",
                Version = "v1",
                Description = "Notification microservice for email and push notifications"
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
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
    }

    private static void AddProviders(IServiceCollection services)
    {
        // Register HttpClient for Brevo API
        services.AddHttpClient<IEmailProvider, BrevoEmailProvider>();
        services.AddSingleton<IEmailTemplateProvider, FileEmailTemplateProvider>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        var assembly = typeof(IAssemblyReference).Assembly;
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Inbox for idempotency
        // This prevents duplicate email sends if Kafka redelivers a message
        DependencyContainer.RegisterServicesWithKafka<NotificationDbContext>(
            services,
            configuration,
            configureRider: rider =>
            {
                // Register consumers in Kafka Rider
                rider.AddConsumer<UserRegisteredConsumer>();
                rider.AddConsumer<PasswordResetRequestedConsumer>();
                rider.AddConsumer<ReferralCreatedConsumer>();
                rider.AddConsumer<LotteryMarketingConsumer>();
                rider.AddConsumer<SendMarketingEmailConsumer>();
                rider.AddConsumer<MarketingUsersResponseConsumer>();

                // Register producers for distributing events
                rider.AddProducer<SendMarketingEmailEvent>(KafkaTopics.SendMarketingEmail);
                rider.AddProducer<GetUsersForMarketingRequestEvent>(KafkaTopics.GetUsersForMarketingRequest);
            },
            configureKafkaEndpoints: (context, kafka) =>
            {
                // Configure topic endpoints using shared constants
                kafka.TopicEndpoint<UserRegisteredEvent>(
                    KafkaTopics.UserRegistered,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<UserRegisteredConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                    });

                kafka.TopicEndpoint<PasswordResetRequestedEvent>(
                    KafkaTopics.PasswordResetRequested,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                    });

                kafka.TopicEndpoint<ReferralCreatedEvent>(
                    KafkaTopics.ReferralCreated,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<ReferralCreatedConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                    });

                kafka.TopicEndpoint<LotteryCreatedEvent>(
                    KafkaTopics.LotteryCreated,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<LotteryMarketingConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                    });

                // Marketing users response - Saga pattern response from Identity service
                kafka.TopicEndpoint<GetUsersForMarketingResponseEvent>(
                    KafkaTopics.GetUsersForMarketingResponse,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<MarketingUsersResponseConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                    });

                // Marketing email distribution topic - multiple consumers can process in parallel
                kafka.TopicEndpoint<SendMarketingEmailEvent>(
                    KafkaTopics.SendMarketingEmail,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<SendMarketingEmailConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                        // Checkpoint every message for reliable processing
                        e.CheckpointInterval = TimeSpan.FromSeconds(1);
                        // Enable concurrent message processing for higher throughput
                        e.ConcurrentMessageLimit = 10;
                    });
            });
    }
}