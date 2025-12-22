using CryptoJackpot.Domain.Core.Bus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Infra.IoC;

public static class DependencyContainer
{
    /// <summary>
    /// Registers shared infrastructure with Kafka.
    /// Allows configuration of both producers and consumers via callbacks.
    /// </summary>
    public static void RegisterServicesWithKafka(
        IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IBusRegistrationConfigurator>? configureBus = null,
        Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? configureKafkaEndpoints = null)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:Host"] ?? "localhost:9092";

        // MassTransit with Kafka
        services.AddMassTransit(x =>
        {
            // Allow microservices to add consumers to the bus
            configureBus?.Invoke(x);

            // In-memory for internal messaging
            x.UsingInMemory((context, cfg) =>
            {
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