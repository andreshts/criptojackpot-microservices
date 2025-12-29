FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Winner/Domain/CryptoJackpot.Winner.Domain.csproj", "Microservices/Winner/Domain/"]
COPY ["Microservices/Winner/Data/CryptoJackpot.Winner.Data.csproj", "Microservices/Winner/Data/"]
COPY ["Microservices/Winner/Application/CryptoJackpot.Winner.Application.csproj", "Microservices/Winner/Application/"]
COPY ["Microservices/Winner/Api/CryptoJackpot.Winner.Api.csproj", "Microservices/Winner/Api/"]

RUN dotnet restore "Microservices/Winner/Api/CryptoJackpot.Winner.Api.csproj"
COPY . .

WORKDIR "/src/Microservices/Winner/Api"
RUN dotnet build "CryptoJackpot.Winner.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Winner.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoJackpot.Winner.Api.dll"]

