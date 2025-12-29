FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Wallet/Domain/CryptoJackpot.Wallet.Domain.csproj", "Microservices/Wallet/Domain/"]
COPY ["Microservices/Wallet/Data/CryptoJackpot.Wallet.Data.csproj", "Microservices/Wallet/Data/"]
COPY ["Microservices/Wallet/Application/CryptoJackpot.Wallet.Application.csproj", "Microservices/Wallet/Application/"]
COPY ["Microservices/Wallet/Api/CryptoJackpot.Wallet.Api.csproj", "Microservices/Wallet/Api/"]

RUN dotnet restore "Microservices/Wallet/Api/CryptoJackpot.Wallet.Api.csproj"
COPY . .

WORKDIR "/src/Microservices/Wallet/Api"
RUN dotnet build "CryptoJackpot.Wallet.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Wallet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoJackpot.Wallet.Api.dll"]

