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
COPY ["Microservices/Winner/Domain/CryptoJackpot.Winner.Domain.csproj", "Microservices/Winner/Domain/"]
COPY ["Microservices/Winner/Data/CryptoJackpot.Winner.Data.csproj", "Microservices/Winner/Data/"]
COPY ["Microservices/Winner/Application/CryptoJackpot.Winner.Application.csproj", "Microservices/Winner/Application/"]
COPY ["Microservices/Winner/Api/CryptoJackpot.Winner.Api.csproj", "Microservices/Winner/Api/"]

# Restaurar dependencias con cache mount
RUN --mount=type=cache,id=nuget-winner,target=/root/.nuget/packages \
    dotnet restore "Microservices/Winner/Api/CryptoJackpot.Winner.Api.csproj"

# Copiar código fuente (solo lo necesario)
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Microservices/Winner/ Microservices/Winner/

WORKDIR "/src/Microservices/Winner/Api"
RUN --mount=type=cache,id=nuget-winner,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Winner.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-winner,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Winner.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Winner.Api.dll"]

