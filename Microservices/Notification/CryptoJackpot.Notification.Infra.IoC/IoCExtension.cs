using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Notification;
using CryptoJackpot.Infra.IoC;
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
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Count > 0)
                {
                    logger.LogInformation("Applying {Count} pending migrations for NotificationDbContext...", pendingMigrations.Count);
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw new InvalidOperationException("Failed to apply migrations for NotificationDbContext in development.", ex);
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

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());
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
        services.AddScoped<IEmailProvider, SmtpEmailProvider>();
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

                // Register producer for distributing individual email jobs
                rider.AddProducer<SendMarketingEmailEvent>(KafkaTopics.SendMarketingEmail);
            },
            configureBus: bus =>
            {
                // Register request client for getting users from Identity service
                bus.AddRequestClient<GetAllUsersRequest>();
            },
            configureKafkaEndpoints: (context, kafka) =>
            {
                // Configure topic endpoints using shared constants
                kafka.TopicEndpoint<UserRegisteredEvent>(
                    KafkaTopics.UserRegistered,
                    KafkaTopics.NotificationGroup,
                    e => e.ConfigureConsumer<UserRegisteredConsumer>(context));

                kafka.TopicEndpoint<PasswordResetRequestedEvent>(
                    KafkaTopics.PasswordResetRequested,
                    KafkaTopics.NotificationGroup,
                    e => e.ConfigureConsumer<PasswordResetRequestedConsumer>(context));

                kafka.TopicEndpoint<ReferralCreatedEvent>(
                    KafkaTopics.ReferralCreated,
                    KafkaTopics.NotificationGroup,
                    e => e.ConfigureConsumer<ReferralCreatedConsumer>(context));

                kafka.TopicEndpoint<LotteryCreatedEvent>(
                    KafkaTopics.LotteryCreated,
                    KafkaTopics.NotificationGroup,
                    e => e.ConfigureConsumer<LotteryMarketingConsumer>(context));

                // Marketing email distribution topic - multiple consumers can process in parallel
                kafka.TopicEndpoint<SendMarketingEmailEvent>(
                    KafkaTopics.SendMarketingEmail,
                    KafkaTopics.NotificationGroup,
                    e =>
                    {
                        e.ConfigureConsumer<SendMarketingEmailConsumer>(context);
                        // Enable concurrent message processing for higher throughput
                        e.ConcurrentMessageLimit = 10;
                    });
            });
    }
}