FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/app/Watcher.Console.App/Watcher.Console.App.csproj", "src/app/Watcher.Console.App/"]
RUN dotnet restore "src/app/Watcher.Console.App/Watcher.Console.App.csproj"
COPY . .
WORKDIR "/src/src/app/Watcher.Console.App"
RUN dotnet build "./Watcher.Console.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Watcher.Console.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Watcher.Console.App.dll"]
