FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DutchMetar.Tools.StubService/DutchMetar.Tools.StubService.csproj", "src/DutchMetar.Tools.StubService/"]
RUN dotnet restore "src/DutchMetar.Tools.StubService/DutchMetar.Tools.StubService.csproj"
COPY . .
WORKDIR "/src/src/DutchMetar.Tools.StubService"
RUN dotnet build "./DutchMetar.Tools.StubService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DutchMetar.Tools.StubService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DutchMetar.Tools.StubService.dll"]
