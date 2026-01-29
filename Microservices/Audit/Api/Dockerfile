FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Create non-root user
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Audit/Domain/CryptoJackpot.Audit.Domain.csproj", "Microservices/Audit/Domain/"]
COPY ["Microservices/Audit/Data/CryptoJackpot.Audit.Data.csproj", "Microservices/Audit/Data/"]
COPY ["Microservices/Audit/Application/CryptoJackpot.Audit.Application.csproj", "Microservices/Audit/Application/"]
COPY ["Microservices/Audit/CryptoJackpot.Audit.Infra.IoC/CryptoJackpot.Audit.Infra.IoC.csproj", "Microservices/Audit/CryptoJackpot.Audit.Infra.IoC/"]
COPY ["Microservices/Audit/Api/CryptoJackpot.Audit.Api.csproj", "Microservices/Audit/Api/"]

RUN dotnet restore "Microservices/Audit/Api/CryptoJackpot.Audit.Api.csproj"
COPY . .

WORKDIR "/src/Microservices/Audit/Api"
RUN dotnet build "CryptoJackpot.Audit.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Audit.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Change ownership and user
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Audit.Api.dll"]
