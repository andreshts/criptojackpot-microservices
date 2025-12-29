FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Lottery/Domain/CryptoJackpot.Lottery.Domain.csproj", "Microservices/Lottery/Domain/"]
COPY ["Microservices/Lottery/Data/CryptoJackpot.Lottery.Data.csproj", "Microservices/Lottery/Data/"]
COPY ["Microservices/Lottery/Application/CryptoJackpot.Lottery.Application.csproj", "Microservices/Lottery/Application/"]
COPY ["Microservices/Lottery/Api/CryptoJackpot.Lottery.Api.csproj", "Microservices/Lottery/Api/"]

# Restaurar dependencias
RUN dotnet restore "Microservices/Lottery/Api/CryptoJackpot.Lottery.Api.csproj"

# Copiar código fuente
COPY . .

# Build
WORKDIR "/src/Microservices/Lottery/Api"
RUN dotnet build "CryptoJackpot.Lottery.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Lottery.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoJackpot.Lottery.Api.dll"]

