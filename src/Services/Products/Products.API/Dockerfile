FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
USER app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy everything (relies on .dockerignore to exclude bin/obj/etc)
COPY . .

# Restore and build
RUN dotnet restore src/Services/Products/Products.API/Products.API.csproj
WORKDIR /src/src/Services/Products/Products.API
RUN dotnet build Products.API.csproj -c $BUILD_CONFIGURATION -o /app/build \
    -p:EnforceCodeStyleInBuild=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Products.API.csproj -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false -p:EnforceCodeStyleInBuild=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Products.API.dll"]
