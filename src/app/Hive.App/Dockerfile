# Use .NET 9.0 ASP.NET base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_ENVIRONMENT=Production

# Use .NET 9.0 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_USERNAME
ARG GITHUB_TOKEN
WORKDIR /src

# Add GitHub NuGet source
RUN dotnet nuget add source --username ${GITHUB_USERNAME} --password ${GITHUB_TOKEN} --store-password-in-clear-text --name github "https://nuget.pkg.github.com/${GITHUB_USERNAME}/index.json"


# Copy all project files and dependencies
COPY src/app/Hive.App/Hive.App.csproj src/app/Hive.App/
COPY src/domain/Domain/Domain.csproj src/domain/Domain/
COPY src/features/Features/Features.csproj src/features/Features/
COPY src/Infrastructure/Infrastructure/Infrastructure.csproj src/Infrastructure/Infrastructure/
COPY src/common/Common/Common.csproj src/common/Common/

# Restore dependencies
RUN dotnet restore src/app/Hive.App/Hive.App.csproj

# Copy the rest of the source code
COPY src/. ./src/

WORKDIR /src/src/app/Hive.App
RUN dotnet build Hive.App.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Hive.App.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
LABEL maintainer="your-email@example.com"
ENTRYPOINT ["dotnet", "Hive.App.dll"]
