namespace CryptoJackpot.Infra.IoC.Configuration;

/// <summary>
/// Configuration for Kafka topic defaults.
/// Bind to "Kafka" section in appsettings.json.
/// </summary>
public class KafkaTopicConfig
{
    public const string SectionName = "Kafka";

    /// <summary>
    /// Kafka bootstrap servers.
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Default number of partitions for auto-created topics.
    /// </summary>
    public int DefaultPartitions { get; set; } = 3;

    /// <summary>
    /// Default replication factor for auto-created topics.
    /// Use 1 for development, 3 for production.
    /// </summary>
    public short DefaultReplicationFactor { get; set; } = 1;
}

