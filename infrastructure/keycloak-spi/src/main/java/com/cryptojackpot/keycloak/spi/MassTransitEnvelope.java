package com.cryptojackpot.keycloak.spi;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.net.InetAddress;
import java.time.Instant;
import java.util.Arrays;
import java.util.List;
import java.util.UUID;

/**
 * MassTransit message envelope format.
 * This wraps messages to be compatible with MassTransit consumers.
 * Uses camelCase for envelope properties as expected by MassTransit.
 */
public class MassTransitEnvelope {

    @JsonProperty("messageId")
    private String messageId;

    @JsonProperty("conversationId")
    private String conversationId;

    @JsonProperty("correlationId")
    private String correlationId;

    @JsonProperty("sourceAddress")
    private String sourceAddress;

    @JsonProperty("destinationAddress")
    private String destinationAddress;

    @JsonProperty("messageType")
    private List<String> messageType;

    @JsonProperty("message")
    private Object message;

    @JsonProperty("sentTime")
    private String sentTime;

    @JsonProperty("host")
    private HostInfo host;

    public MassTransitEnvelope(KeycloakUserCreatedEvent event) {
        this.messageId = UUID.randomUUID().toString();
        this.conversationId = UUID.randomUUID().toString();
        this.correlationId = event.getCorrelationId();
        this.sourceAddress = "keycloak://event-listener";
        this.destinationAddress = "kafka://keycloak-user-created";
        
        // MassTransit expects message types in URN format (full type hierarchy)
        this.messageType = Arrays.asList(
            "urn:message:CryptoJackpot.Domain.Core.IntegrationEvents.Identity:KeycloakUserCreatedEvent",
            "urn:message:CryptoJackpot.Domain.Core.Events:Event"
        );
        
        this.message = event;
        this.sentTime = Instant.now().toString();
        this.host = new HostInfo();
    }

    // Getters
    public String getMessageId() { return messageId; }
    public String getConversationId() { return conversationId; }
    public String getCorrelationId() { return correlationId; }
    public String getSourceAddress() { return sourceAddress; }
    public String getDestinationAddress() { return destinationAddress; }
    public List<String> getMessageType() { return messageType; }
    public Object getMessage() { return message; }
    public String getSentTime() { return sentTime; }
    public HostInfo getHost() { return host; }

    public static class HostInfo {
        @JsonProperty("machineName")
        private String machineName;

        @JsonProperty("processName")
        private String processName;

        @JsonProperty("processId")
        private int processId;

        public HostInfo() {
            try {
                this.machineName = InetAddress.getLocalHost().getHostName();
            } catch (Exception e) {
                this.machineName = "keycloak";
            }
            this.processName = "keycloak";
            this.processId = (int) ProcessHandle.current().pid();
        }

        public String getMachineName() { return machineName; }
        public String getProcessName() { return processName; }
        public int getProcessId() { return processId; }
    }
}

