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
COPY ["Microservices/Identity/Domain/CryptoJackpot.Identity.Domain.csproj", "Microservices/Identity/Domain/"]
COPY ["Microservices/Identity/Data/CryptoJackpot.Identity.Data.csproj", "Microservices/Identity/Data/"]
COPY ["Microservices/Identity/Application/CryptoJackpot.Identity.Application.csproj", "Microservices/Identity/Application/"]
COPY ["Microservices/Identity/CryptoJackpot.Identity.Infra.IoC/CryptoJackpot.Identity.Infra.IoC.csproj", "Microservices/Identity/CryptoJackpot.Identity.Infra.IoC/"]
COPY ["Microservices/Identity/Api/CryptoJackpot.Identity.Api.csproj", "Microservices/Identity/Api/"]

# Restaurar dependencias con cache mount
RUN --mount=type=cache,id=nuget-identity,target=/root/.nuget/packages \
    dotnet restore "Microservices/Identity/Api/CryptoJackpot.Identity.Api.csproj"

# Copiar código fuente
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Microservices/Identity/ Microservices/Identity/

# Build con cache mount
WORKDIR "/src/Microservices/Identity/Api"
RUN --mount=type=cache,id=nuget-identity,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Identity.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-identity,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Identity.Api.dll"]

