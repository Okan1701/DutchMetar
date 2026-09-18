FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG APP_VERSION="0.1.0"
ARG APP_VERSION_SUFFIX="dev"
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish ./src/DutchMetar.WorkerService -c Release -o out --no-restore /p:VersionPrefix=${APP_VERSION} /p:VersionSuffix=${APP_VERSION_SUFFIX}

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "DutchMetar.WorkerService.dll"]