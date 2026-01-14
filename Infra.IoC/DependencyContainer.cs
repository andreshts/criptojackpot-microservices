using CryptoJackpot.Domain.Core.Bus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Infra.IoC;

public static class DependencyContainer
{
    /// <summary>
    /// Registers shared infrastructure with Kafka and Transactional Outbox.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type for the microservice</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="configureRider">Configure rider (add producers/consumers)</param>
    /// <param name="configureBus">Configure bus (add consumers)</param>
    /// <param name="configureKafkaEndpoints">Configure Kafka topic endpoints (for consumers)</param>
    /// <param name="useMessageScheduler">Enable in-memory message scheduler for delayed messages</param>
    public static void RegisterServicesWithKafka<TDbContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IBusRegistrationConfigurator>? configureBus = null,
        Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? configureKafkaEndpoints = null,
        bool useMessageScheduler = false)
        where TDbContext : DbContext
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        // MassTransit with Kafka and Outbox
        services.AddMassTransit(x =>
        {
            // Allow microservices to add consumers to the bus
            configureBus?.Invoke(x);

            // Configure Entity Framework Outbox for transactional consistency
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                // Use PostgreSQL
                o.UsePostgres();
                
                // Query delay for polling outbox messages
                o.QueryDelay = TimeSpan.FromSeconds(1);
                
                // Enable the bus outbox so messages are published via the outbox
                o.UseBusOutbox();
            });

            // In-memory for internal messaging with optional scheduler
            x.UsingInMemory((context, cfg) =>
            {
                if (useMessageScheduler)
                {
                    // Enable in-memory message scheduler for delayed/scheduled messages
                    cfg.UseDelayedMessageScheduler();
                }
                
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider for external events
            x.AddRider(rider =>
            {
                // Allow microservices to configure producers/consumers
                configureRider?.Invoke(rider);

                rider.UsingKafka((context, kafka) =>
                {
                    kafka.Host(kafkaHost);
                    
                    // Allow microservices to configure topic endpoints
                    configureKafkaEndpoints?.Invoke(context, kafka);
                });
            });
        });
    }

    /// <summary>
    /// Registers shared infrastructure with Kafka (without Outbox - for microservices that don't need it).
    /// </summary>
    public static void RegisterServicesWithKafka(
        IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IBusRegistrationConfigurator>? configureBus = null,
        Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? configureKafkaEndpoints = null,
        bool useMessageScheduler = false)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        // MassTransit with Kafka
        services.AddMassTransit(x =>
        {
            // Allow microservices to add consumers to the bus
            configureBus?.Invoke(x);

            // In-memory for internal messaging with optional scheduler
            x.UsingInMemory((context, cfg) =>
            {
                if (useMessageScheduler)
                {
                    // Enable in-memory message scheduler for delayed/scheduled messages
                    cfg.UseDelayedMessageScheduler();
                }
                
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider for external events
            x.AddRider(rider =>
            {
                // Allow microservices to configure producers/consumers
                configureRider?.Invoke(rider);

                rider.UsingKafka((context, kafka) =>
                {
                    kafka.Host(kafkaHost);
                    
                    // Allow microservices to configure topic endpoints
                    configureKafkaEndpoints?.Invoke(context, kafka);
                });
            });
        });
    }
}