# 1. Build faza
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiranje .csproj fajlova (bitno za keširanje lejera)
COPY tour-service/TouristApp/src/TouristApp.API/TouristApp.API.csproj TouristApp.API/
COPY tour-service/TouristApp/src/Modules/Tours/TouristApp.Tours.API/TouristApp.Tours.API.csproj Modules/Tours/TouristApp.Tours.API/
COPY tour-service/TouristApp/src/Modules/Tours/TouristApp.Tours.Core/TouristApp.Tours.Core.csproj Modules/Tours/TouristApp.Tours.Core/
COPY tour-service/TouristApp/src/Modules/Tours/TouristApp.Tours.Infrastructure/TouristApp.Tours.Infrastructure.csproj Modules/Tours/TouristApp.Tours.Infrastructure/
COPY tour-service/TouristApp/src/BuildingBlocks/TouristApp.BuildingBlocks.Core/TouristApp.BuildingBlocks.Core.csproj BuildingBlocks/TouristApp.BuildingBlocks.Core/
COPY tour-service/TouristApp/src/BuildingBlocks/TouristApp.BuildingBlocks.Infrastructure/TouristApp.BuildingBlocks.Infrastructure.csproj BuildingBlocks/TouristApp.BuildingBlocks.Infrastructure/

# Restore zavisnosti
RUN dotnet restore TouristApp.API/TouristApp.API.csproj

# Kopiranje ostatka koda
COPY tour-service/TouristApp/src/TouristApp.API/ TouristApp.API/
COPY tour-service/TouristApp/src/Modules/Tours/ Modules/Tours/
COPY tour-service/TouristApp/src/BuildingBlocks/ BuildingBlocks/

# Publish aplikacije
RUN dotnet publish TouristApp.API/TouristApp.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# 2. Runtime faza
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Kopiranje objavljenih fajlova iz build faze
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "TouristApp.API.dll"]