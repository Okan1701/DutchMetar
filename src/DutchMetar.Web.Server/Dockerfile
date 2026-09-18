# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG APP_VERSION="0.1.0"
ARG APP_VERSION_SUFFIX="dev"

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_24.x | bash
RUN apt-get -y install nodejs
RUN npm install -g @angular/cli

FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION
ARG APP_VERSION_SUFFIX
WORKDIR /dutchmetar
COPY . ./
RUN sed -i "s|%APP_VERSION%|${APP_VERSION}-${APP_VERSION_SUFFIX}|g" /dutchmetar/src/DutchMetar.Web.Client/src/environments/environment.ts
RUN dotnet restore "./src/DutchMetar.Web.Server/DutchMetar.Web.Server.csproj"
RUN dotnet publish "./src/DutchMetar.Web.Server/DutchMetar.Web.Server.csproj" --no-restore -c $BUILD_CONFIGURATION /p:VersionPrefix=${APP_VERSION} /p:VersionSuffix=${APP_VERSION_SUFFIX} -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DutchMetar.Web.Server.dll"]
