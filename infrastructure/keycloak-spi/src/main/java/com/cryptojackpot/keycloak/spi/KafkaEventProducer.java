package com.cryptojackpot.keycloak.spi;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.apache.kafka.common.serialization.StringSerializer;
import org.jboss.logging.Logger;

import java.util.Properties;
import java.util.concurrent.TimeUnit;

/**
 * Kafka producer singleton for publishing events from Keycloak.
 * Configured via environment variables.
 */
public class KafkaEventProducer {

    private static final Logger LOG = Logger.getLogger(KafkaEventProducer.class);
    
    private static KafkaEventProducer instance;
    
    private final KafkaProducer<String, String> producer;
    private final ObjectMapper objectMapper;
    private final String topicUserCreated;

    private KafkaEventProducer() {
        String bootstrapServers = getEnvOrDefault("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092");
        this.topicUserCreated = getEnvOrDefault("KAFKA_TOPIC_USER_CREATED", "keycloak-user-created");

        Properties props = new Properties();
        props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
        props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.ACKS_CONFIG, "all");
        props.put(ProducerConfig.RETRIES_CONFIG, 3);
        props.put(ProducerConfig.RETRY_BACKOFF_MS_CONFIG, 1000);
        props.put(ProducerConfig.MAX_BLOCK_MS_CONFIG, 5000);
        
        // Connection timeout settings
        props.put(ProducerConfig.REQUEST_TIMEOUT_MS_CONFIG, 5000);
        props.put(ProducerConfig.DELIVERY_TIMEOUT_MS_CONFIG, 30000);

        this.producer = new KafkaProducer<>(props);
        
        this.objectMapper = new ObjectMapper();
        this.objectMapper.registerModule(new JavaTimeModule());
        this.objectMapper.disable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS);

        LOG.infof("Kafka producer initialized. Bootstrap servers: %s, Topic: %s", 
                  bootstrapServers, topicUserCreated);
    }

    public static synchronized KafkaEventProducer getInstance() {
        if (instance == null) {
            instance = new KafkaEventProducer();
        }
        return instance;
    }

    public void publishUserCreatedEvent(KeycloakUserCreatedEvent event) {
        try {
            String json = objectMapper.writeValueAsString(event);
            ProducerRecord<String, String> record = new ProducerRecord<>(
                topicUserCreated, 
                event.getKeycloakId(), 
                json
            );

            producer.send(record, (metadata, exception) -> {
                if (exception != null) {
                    LOG.errorf(exception, "Failed to publish user created event for: %s", event.getEmail());
                } else {
                    LOG.infof("Published user created event to %s partition %d offset %d for: %s",
                             metadata.topic(), metadata.partition(), metadata.offset(), event.getEmail());
                }
            });
            
        } catch (JsonProcessingException e) {
            LOG.errorf(e, "Failed to serialize user created event for: %s", event.getEmail());
        }
    }

    public void close() {
        if (producer != null) {
            try {
                producer.close(java.time.Duration.ofSeconds(5));
                LOG.info("Kafka producer closed");
            } catch (Exception e) {
                LOG.warn("Error closing Kafka producer", e);
            }
        }
    }

    private static String getEnvOrDefault(String name, String defaultValue) {
        String value = System.getenv(name);
        return (value != null && !value.isEmpty()) ? value : defaultValue;
    }
}

