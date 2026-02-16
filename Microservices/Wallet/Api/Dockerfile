FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Crear usuario non-root
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Wallet/Domain/CryptoJackpot.Wallet.Domain.csproj", "Microservices/Wallet/Domain/"]
COPY ["Microservices/Wallet/Data/CryptoJackpot.Wallet.Data.csproj", "Microservices/Wallet/Data/"]
COPY ["Microservices/Wallet/Application/CryptoJackpot.Wallet.Application.csproj", "Microservices/Wallet/Application/"]
COPY ["Microservices/Wallet/CryptoJackpot.Wallet.Infra.IoC/CryptoJackpot.Wallet.Infra.IoC.csproj", "Microservices/Wallet/CryptoJackpot.Wallet.Infra.IoC/"]
COPY ["Microservices/Wallet/Api/CryptoJackpot.Wallet.Api.csproj", "Microservices/Wallet/Api/"]

# Restaurar dependencias con cache mount
RUN --mount=type=cache,id=nuget-wallet,target=/root/.nuget/packages \
    dotnet restore "Microservices/Wallet/Api/CryptoJackpot.Wallet.Api.csproj"

# Copiar código fuente (solo lo necesario)
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Microservices/Wallet/ Microservices/Wallet/

WORKDIR "/src/Microservices/Wallet/Api"
RUN --mount=type=cache,id=nuget-wallet,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Wallet.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-wallet,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Wallet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Wallet.Api.dll"]

