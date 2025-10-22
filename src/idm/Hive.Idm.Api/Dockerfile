FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_ENVIRONMENT=Production
ENV RabbitMQ__HostName=rabbitmq
ENV RabbitMQ__UserName=guest
ENV RabbitMQ__Password=guest

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore as distinct layers
COPY ["src/idm/Hive.Idm.Api/Hive.Idm.Api.csproj", "src/idm/Hive.Idm.Api/"]
COPY src/domain/Domain/Domain.csproj src/domain/Domain/
COPY src/features/Features/Features.csproj src/features/Features/
COPY src/Infrastructure/Infrastructure/Infrastructure.csproj src/Infrastructure/Infrastructure/
COPY src/common/Common/Common.csproj src/common/Common/

# Restore dependencies
RUN dotnet restore "src/idm/Hive.Idm.Api/Hive.Idm.Api.csproj"

COPY . .

WORKDIR "/src/src/idm/Hive.Idm.Api"
RUN dotnet build "./Hive.Idm.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hive.Idm.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hive.Idm.Api.dll"]
