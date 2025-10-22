FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV RabbitMQ__Host=watcher-rabbitmq
ENV RabbitMQ__UserName=guest
ENV RabbitMQ__Password=guest

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/app/Watcher.Console.App/Watcher.Console.App.csproj", "src/app/Watcher.Console.App/"]
COPY src/domain/Domain/Domain.csproj src/domain/Domain/
COPY src/features/Features/Features.csproj src/features/Features/
COPY src/Infrastructure/Infrastructure/Infrastructure.csproj src/Infrastructure/Infrastructure/
COPY src/common/Common/Common.csproj src/common/Common/

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
