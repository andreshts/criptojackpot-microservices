FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Identity/Domain/CryptoJackpot.Identity.Domain.csproj", "Microservices/Identity/Domain/"]
COPY ["Microservices/Identity/Data/CryptoJackpot.Identity.Data.csproj", "Microservices/Identity/Data/"]
COPY ["Microservices/Identity/Application/CryptoJackpot.Identity.Application.csproj", "Microservices/Identity/Application/"]
COPY ["Microservices/Identity/Api/CryptoJackpot.Identity.Api.csproj", "Microservices/Identity/Api/"]

# Restaurar dependencias
RUN dotnet restore "Microservices/Identity/Api/CryptoJackpot.Identity.Api.csproj"

# Copiar código fuente
COPY . .

# Build
WORKDIR "/src/Microservices/Identity/Api"
RUN dotnet build "CryptoJackpot.Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoJackpot.Identity.Api.dll"]

