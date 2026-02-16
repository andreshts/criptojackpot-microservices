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
COPY ["Microservices/Order/Domain/CryptoJackpot.Order.Domain.csproj", "Microservices/Order/Domain/"]
COPY ["Microservices/Order/Data/CryptoJackpot.Order.Data.csproj", "Microservices/Order/Data/"]
COPY ["Microservices/Order/Application/CryptoJackpot.Order.Application.csproj", "Microservices/Order/Application/"]
COPY ["Microservices/Order/CryptoJackpot.Order.Infra.IoC/CryptoJackpot.Order.Infra.IoC.csproj", "Microservices/Order/CryptoJackpot.Order.Infra.IoC/"]
COPY ["Microservices/Order/Api/CryptoJackpot.Order.Api.csproj", "Microservices/Order/Api/"]

# Restaurar dependencias con cache mount
RUN --mount=type=cache,id=nuget-order,target=/root/.nuget/packages \
    dotnet restore "Microservices/Order/Api/CryptoJackpot.Order.Api.csproj"

# Copiar código fuente (solo lo necesario)
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Microservices/Order/ Microservices/Order/

WORKDIR "/src/Microservices/Order/Api"
RUN --mount=type=cache,id=nuget-order,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Order.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-order,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Order.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Order.Api.dll"]

