FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app
USER app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

RUN dotnet restore src/Services/Notification/EShop.NotificationService.csproj
WORKDIR /src/src/Services/Notification
RUN dotnet build EShop.NotificationService.csproj -c $BUILD_CONFIGURATION -o /app/build \
    -p:EnforceCodeStyleInBuild=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish EShop.NotificationService.csproj -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false -p:EnforceCodeStyleInBuild=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.NotificationService.dll"]
