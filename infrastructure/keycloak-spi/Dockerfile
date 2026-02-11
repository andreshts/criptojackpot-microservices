# Keycloak with Kafka Event Listener SPI
# This Dockerfile extends the official Keycloak image and adds the custom SPI

# Stage 1: Build the SPI JAR with Maven
FROM maven:3.9-eclipse-temurin-17 AS maven-builder

WORKDIR /build
COPY pom.xml .
COPY src ./src

# Build the JAR (skip tests for faster build)
RUN mvn clean package -DskipTests

# Stage 2: Build optimized Keycloak with the SPI
FROM quay.io/keycloak/keycloak:26.0.0 AS keycloak-builder

# Copy the SPI JAR from Maven build stage
COPY --from=maven-builder /build/target/keycloak-kafka-event-listener-1.0.0.jar /opt/keycloak/providers/

# Build the optimized Keycloak distribution
RUN /opt/keycloak/bin/kc.sh build

# Stage 3: Final image
FROM quay.io/keycloak/keycloak:26.0.0

# Copy the built providers from builder stage
COPY --from=keycloak-builder /opt/keycloak/providers/ /opt/keycloak/providers/
COPY --from=keycloak-builder /opt/keycloak/lib/quarkus/ /opt/keycloak/lib/quarkus/

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]

