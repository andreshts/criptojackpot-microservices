using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Consumers;
using CryptoJackpot.Notification.Application.Handlers.Commands;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Application.Providers;
using CryptoJackpot.Notification.Application.Services;
using CryptoJackpot.Notification.Data.Context;
using CryptoJackpot.Notification.Data.Repositories;
using CryptoJackpot.Notification.Domain.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CryptoJackpot.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddConfiguration(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services);
        AddRepositories(services);
        AddProviders(services);
        AddApplicationServices(services);
        AddInfrastructure(services, configuration);

        return services;
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
            options.UseNpgsql(connectionString));
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

    private static void AddControllers(IServiceCollection services)
    {
        services.AddControllers();
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
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SendEmailConfirmationHandler).Assembly));

        // Application Services
        services.AddScoped<INotificationService, NotificationService>();
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Infra.Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:Host"] ?? "localhost:9092";

        // MassTransit with Kafka
        services.AddMassTransit(x =>
        {
            // Register consumers
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<PasswordResetRequestedConsumer>();
            x.AddConsumer<ReferralCreatedConsumer>();

            // In-memory for internal
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider for external events
            x.AddRider(rider =>
            {
                rider.AddConsumer<UserRegisteredConsumer>();
                rider.AddConsumer<PasswordResetRequestedConsumer>();
                rider.AddConsumer<ReferralCreatedConsumer>();

                rider.UsingKafka((context, kafka) =>
                {
                    kafka.Host(kafkaHost);

                    kafka.TopicEndpoint<UserRegisteredEvent>("user-registered", "notification-group", e =>
                    {
                        e.ConfigureConsumer<UserRegisteredConsumer>(context);
                    });

                    kafka.TopicEndpoint<PasswordResetRequestedEvent>("password-reset-requested", "notification-group", e =>
                    {
                        e.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
                    });

                    kafka.TopicEndpoint<ReferralCreatedEvent>("referral-created", "notification-group", e =>
                    {
                        e.ConfigureConsumer<ReferralCreatedConsumer>(context);
                    });
                });
            });
        });
    }
}
