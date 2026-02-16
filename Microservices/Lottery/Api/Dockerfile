FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Crear usuario non-root
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Lottery/Domain/CryptoJackpot.Lottery.Domain.csproj", "Microservices/Lottery/Domain/"]
COPY ["Microservices/Lottery/Data/CryptoJackpot.Lottery.Data.csproj", "Microservices/Lottery/Data/"]
COPY ["Microservices/Lottery/Application/CryptoJackpot.Lottery.Application.csproj", "Microservices/Lottery/Application/"]
COPY ["Microservices/Lottery/CryptoJackpot.Lottery.Infra.IoC/CryptoJackpot.Lottery.Infra.IoC.csproj", "Microservices/Lottery/CryptoJackpot.Lottery.Infra.IoC/"]
COPY ["Microservices/Lottery/Api/CryptoJackpot.Lottery.Api.csproj", "Microservices/Lottery/Api/"]

# Restaurar dependencias con cache mount
RUN --mount=type=cache,id=nuget-lottery,target=/root/.nuget/packages \
    dotnet restore "Microservices/Lottery/Api/CryptoJackpot.Lottery.Api.csproj"

# Copiar código fuente (solo lo necesario)
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Microservices/Lottery/ Microservices/Lottery/

# Build con cache mount
WORKDIR "/src/Microservices/Lottery/Api"
RUN --mount=type=cache,id=nuget-lottery,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Lottery.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-lottery,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Lottery.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Lottery.Api.dll"]

