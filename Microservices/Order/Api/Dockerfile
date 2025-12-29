FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Order/Domain/CryptoJackpot.Order.Domain.csproj", "Microservices/Order/Domain/"]
COPY ["Microservices/Order/Data/CryptoJackpot.Order.Data.csproj", "Microservices/Order/Data/"]
COPY ["Microservices/Order/Application/CryptoJackpot.Order.Application.csproj", "Microservices/Order/Application/"]
COPY ["Microservices/Order/Api/CryptoJackpot.Order.Api.csproj", "Microservices/Order/Api/"]

RUN dotnet restore "Microservices/Order/Api/CryptoJackpot.Order.Api.csproj"
COPY . .

WORKDIR "/src/Microservices/Order/Api"
RUN dotnet build "CryptoJackpot.Order.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Order.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoJackpot.Order.Api.dll"]

