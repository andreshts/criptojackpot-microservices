using MassTransit;
using Microsoft.Extensions.Configuration;

namespace CryptoJackpot.Infra.IoC.Extensions;

/// <summary>
/// Extension methods for Kafka topic configuration.
/// </summary>
public static class KafkaTopicExtensions
{
    /// <summary>
    /// Configures topic defaults including auto-creation with partitions and replication factor.
    /// </summary>
    /// <param name="endpoint">The Kafka topic endpoint configurator.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <typeparam name="TKey">The key type for the topic.</typeparam>
    /// <typeparam name="TValue">The message type for the topic.</typeparam>
    public static void ConfigureTopicDefaults<TKey, TValue>(
        this IKafkaTopicReceiveEndpointConfigurator<TKey, TValue> endpoint,
        IConfiguration configuration)
        where TValue : class
    {
        var partitions = configuration.GetValue("Kafka:DefaultPartitions", 3);
        var replicationFactor = configuration.GetValue("Kafka:DefaultReplicationFactor", 1);

        endpoint.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
        
        // Auto-create topic if it doesn't exist
        endpoint.CreateIfMissing(t =>
        {
            t.NumPartitions = (ushort)partitions;
            t.ReplicationFactor = (short)replicationFactor;
        });
    }

    /// <summary>
    /// Configures topic defaults with custom partition and replication settings.
    /// </summary>
    /// <param name="endpoint">The Kafka topic endpoint configurator.</param>
    /// <param name="partitions">Number of partitions for the topic.</param>
    /// <param name="replicationFactor">Replication factor for the topic.</param>
    /// <typeparam name="TKey">The key type for the topic.</typeparam>
    /// <typeparam name="TValue">The message type for the topic.</typeparam>
    public static void ConfigureTopicDefaults<TKey, TValue>(
        this IKafkaTopicReceiveEndpointConfigurator<TKey, TValue> endpoint,
        int partitions,
        short replicationFactor)
        where TValue : class
    {
        endpoint.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
        
        // Auto-create topic if it doesn't exist
        endpoint.CreateIfMissing(t =>
        {
            t.NumPartitions = (ushort)partitions;
            t.ReplicationFactor = replicationFactor;
        });
    }
}

