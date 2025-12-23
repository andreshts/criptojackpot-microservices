using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Infra.IoC;
using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Consumers;
using CryptoJackpot.Notification.Application.Handlers.Commands;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Application.Providers;
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
            });
    }
}
