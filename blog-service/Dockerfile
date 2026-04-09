# blog-service/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TouristApp/src/TouristApp.API/TouristApp.API.csproj                                                         TouristApp.API/
COPY TouristApp/src/Modules/Blog/TouristApp.Blog.API/TouristApp.Blog.API.csproj                                  Modules/Blog/TouristApp.Blog.API/
COPY TouristApp/src/Modules/Blog/TouristApp.Blog.Core/TouristApp.Blog.Core.csproj                                Modules/Blog/TouristApp.Blog.Core/
COPY TouristApp/src/Modules/Blog/TouristApp.Blog.Infrastructure/TouristApp.Blog.Infrastructure.csproj            Modules/Blog/TouristApp.Blog.Infrastructure/
COPY TouristApp/src/BuildingBlocks/TouristApp.BuildingBlocks.Core/TouristApp.BuildingBlocks.Core.csproj          BuildingBlocks/TouristApp.BuildingBlocks.Core/
COPY TouristApp/src/BuildingBlocks/TouristApp.BuildingBlocks.Infrastructure/TouristApp.BuildingBlocks.Infrastructure.csproj  BuildingBlocks/TouristApp.BuildingBlocks.Infrastructure/

RUN dotnet restore TouristApp.API/TouristApp.API.csproj

COPY TouristApp/src/TouristApp.API/                        TouristApp.API/
COPY TouristApp/src/Modules/Blog/                          Modules/Blog/
COPY TouristApp/src/BuildingBlocks/                        BuildingBlocks/

RUN dotnet publish TouristApp.API/TouristApp.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "TouristApp.API.dll"]