# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files for restore
COPY ["CryptoJackpotDistributed.sln", "."]
COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Tools/CryptoJackpot.Migrator/CryptoJackpot.Migrator.csproj", "Tools/CryptoJackpot.Migrator/"]

# Copy microservice projects
COPY ["Microservices/Identity/Domain/CryptoJackpot.Identity.Domain.csproj", "Microservices/Identity/Domain/"]
COPY ["Microservices/Identity/Data/CryptoJackpot.Identity.Data.csproj", "Microservices/Identity/Data/"]
COPY ["Microservices/Identity/Application/CryptoJackpot.Identity.Application.csproj", "Microservices/Identity/Application/"]

COPY ["Microservices/Lottery/Domain/CryptoJackpot.Lottery.Domain.csproj", "Microservices/Lottery/Domain/"]
COPY ["Microservices/Lottery/Data/CryptoJackpot.Lottery.Data.csproj", "Microservices/Lottery/Data/"]
COPY ["Microservices/Lottery/Application/CryptoJackpot.Lottery.Application.csproj", "Microservices/Lottery/Application/"]

COPY ["Microservices/Order/Domain/CryptoJackpot.Order.Domain.csproj", "Microservices/Order/Domain/"]
COPY ["Microservices/Order/Data/CryptoJackpot.Order.Data.csproj", "Microservices/Order/Data/"]
COPY ["Microservices/Order/Application/CryptoJackpot.Order.Application.csproj", "Microservices/Order/Application/"]

COPY ["Microservices/Wallet/Domain/CryptoJackpot.Wallet.Domain.csproj", "Microservices/Wallet/Domain/"]
COPY ["Microservices/Wallet/Data/CryptoJackpot.Wallet.Data.csproj", "Microservices/Wallet/Data/"]
COPY ["Microservices/Wallet/Application/CryptoJackpot.Wallet.Application.csproj", "Microservices/Wallet/Application/"]

COPY ["Microservices/Winner/Domain/CryptoJackpot.Winner.Domain.csproj", "Microservices/Winner/Domain/"]
COPY ["Microservices/Winner/Data/CryptoJackpot.Winner.Data.csproj", "Microservices/Winner/Data/"]
COPY ["Microservices/Winner/Application/CryptoJackpot.Winner.Application.csproj", "Microservices/Winner/Application/"]

COPY ["Microservices/Notification/Domain/CryptoJackpot.Notification.Domain.csproj", "Microservices/Notification/Domain/"]
COPY ["Microservices/Notification/Data/CryptoJackpot.Notification.Data.csproj", "Microservices/Notification/Data/"]
COPY ["Microservices/Notification/Application/CryptoJackpot.Notification.Application.csproj", "Microservices/Notification/Application/"]

# Restore dependencies con cache mount
RUN --mount=type=cache,id=nuget-migrator,target=/root/.nuget/packages \
    dotnet restore "Tools/CryptoJackpot.Migrator/CryptoJackpot.Migrator.csproj"

# Copy source code (solo lo necesario)
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Tools/CryptoJackpot.Migrator/ Tools/CryptoJackpot.Migrator/
COPY Microservices/Identity/Domain/ Microservices/Identity/Domain/
COPY Microservices/Identity/Data/ Microservices/Identity/Data/
COPY Microservices/Identity/Application/ Microservices/Identity/Application/
COPY Microservices/Lottery/Domain/ Microservices/Lottery/Domain/
COPY Microservices/Lottery/Data/ Microservices/Lottery/Data/
COPY Microservices/Lottery/Application/ Microservices/Lottery/Application/
COPY Microservices/Order/Domain/ Microservices/Order/Domain/
COPY Microservices/Order/Data/ Microservices/Order/Data/
COPY Microservices/Order/Application/ Microservices/Order/Application/
COPY Microservices/Wallet/Domain/ Microservices/Wallet/Domain/
COPY Microservices/Wallet/Data/ Microservices/Wallet/Data/
COPY Microservices/Wallet/Application/ Microservices/Wallet/Application/
COPY Microservices/Winner/Domain/ Microservices/Winner/Domain/
COPY Microservices/Winner/Data/ Microservices/Winner/Data/
COPY Microservices/Winner/Application/ Microservices/Winner/Application/
COPY Microservices/Notification/Domain/ Microservices/Notification/Domain/
COPY Microservices/Notification/Data/ Microservices/Notification/Data/
COPY Microservices/Notification/Application/ Microservices/Notification/Application/

# Build
WORKDIR "/src/Tools/CryptoJackpot.Migrator"
RUN --mount=type=cache,id=nuget-migrator,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,id=nuget-migrator,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# Runtime stage - minimal image for running migrations
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user for security
USER app

ENTRYPOINT ["dotnet", "CryptoJackpot.Migrator.dll"]

