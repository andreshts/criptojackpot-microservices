package com.cryptojackpot.keycloak.spi;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.time.Instant;
import java.util.Map;
import java.util.UUID;

/**
 * Event published to Kafka when a user is created in Keycloak.
 * Matches the C# KeycloakUserCreatedEvent structure.
 * Uses PascalCase for JSON properties to match C# naming conventions.
 */
public class KeycloakUserCreatedEvent {

    @JsonProperty("Timestamp")
    private Instant timestamp;

    @JsonProperty("CorrelationId")
    private String correlationId;

    @JsonProperty("KeycloakId")
    private String keycloakId;

    @JsonProperty("Email")
    private String email;

    @JsonProperty("FirstName")
    private String firstName;

    @JsonProperty("LastName")
    private String lastName;

    @JsonProperty("EmailVerified")
    private boolean emailVerified;

    @JsonProperty("Attributes")
    private Map<String, String> attributes;

    public KeycloakUserCreatedEvent() {
        this.timestamp = Instant.now();
        this.correlationId = UUID.randomUUID().toString();
    }

    // Builder pattern for clean construction
    public static Builder builder() {
        return new Builder();
    }

    public static class Builder {
        private final KeycloakUserCreatedEvent event = new KeycloakUserCreatedEvent();

        public Builder keycloakId(String keycloakId) {
            event.keycloakId = keycloakId;
            return this;
        }

        public Builder email(String email) {
            event.email = email;
            return this;
        }

        public Builder firstName(String firstName) {
            event.firstName = firstName;
            return this;
        }

        public Builder lastName(String lastName) {
            event.lastName = lastName;
            return this;
        }

        public Builder emailVerified(boolean emailVerified) {
            event.emailVerified = emailVerified;
            return this;
        }

        public Builder attributes(Map<String, String> attributes) {
            event.attributes = attributes;
            return this;
        }

        public KeycloakUserCreatedEvent build() {
            return event;
        }
    }

    // Getters
    public Instant getTimestamp() { return timestamp; }
    public String getCorrelationId() { return correlationId; }
    public String getKeycloakId() { return keycloakId; }
    public String getEmail() { return email; }
    public String getFirstName() { return firstName; }
    public String getLastName() { return lastName; }
    public boolean isEmailVerified() { return emailVerified; }
    public Map<String, String> getAttributes() { return attributes; }
}

