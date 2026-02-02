FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

RUN dotnet restore src/Services/Products/Products.API/EShop.Products.API.csproj
WORKDIR /src/src/Services/Products/Products.API
RUN dotnet build EShop.Products.API.csproj -c $BUILD_CONFIGURATION -o /app/build \
    -p:EnforceCodeStyleInBuild=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish EShop.Products.API.csproj -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false -p:EnforceCodeStyleInBuild=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.Products.API.dll"]
