# BFF Gateway with YARP Reverse Proxy
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Create non-root user
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["Microservices/BFF/CryptoJackpot.BFF.csproj", "Microservices/BFF/"]

# Restore dependencies with cache mount
RUN --mount=type=cache,id=nuget-bff,target=/root/.nuget/packages \
    dotnet restore "Microservices/BFF/CryptoJackpot.BFF.csproj"

# Copy source code
COPY Microservices/BFF/ Microservices/BFF/

# Build with cache mount
WORKDIR "/src/Microservices/BFF"
RUN --mount=type=cache,id=nuget-bff,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.BFF.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-bff,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.BFF.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Change ownership and user
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.BFF.dll"]

