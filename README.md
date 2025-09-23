# Hive Movie Platform

A modular .NET 9.0 application for movie platform management with file system monitoring capabilities.

## 🎬 Overview

Hive is a movie platform solution built using clean architecture principles, featuring:
- **Web API**: RESTful API for movie platform operations
- **File System Watcher**: Console application for monitoring file system changes
- **Modular Architecture**: Organized into domain, infrastructure, and feature layers
- **Docker Support**: Containerized deployment ready

## 🏗️ Architecture

The solution follows Domain-Driven Design (DDD) and Clean Architecture patterns:

```
src/
├── app/                    # Application layer
│   ├── Hive.App/          # Web API application
│   └── Watcher.Console.App/ # File system watcher console app
├── common/                 # Shared common utilities
├── domain/                 # Domain layer (business logic)
├── features/              # Feature implementations
└── Infrastructure/        # Infrastructure layer (data access, external services)

tests/                     # Unit and integration tests
├── Console.App.Tests/     # Tests for console application
└── Domain.Tests/          # Tests for domain layer
```

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### Building the Solution

```bash
# Clone the repository
git clone <repository-url>
cd hive-movie-platfrom

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running the Applications

#### Web API (Hive.App)

```bash
# Navigate to the API project
cd src/app/Hive.App

# Run the application
dotnet run

# The API will be available at:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
```

#### File System Watcher

```bash
# Navigate to the console app
cd src/app/Watcher.Console.App

# Run with folder watching
dotnet run -- --watch /path/to/folder

# Or use short form
dotnet run -- -w /path/to/folder

# Display help
dotnet run -- --help
```

## 🐳 Docker Deployment

### Web API

```bash
# Build the Docker image
docker build -f src/app/Hive.App/Dockerfile -t hive-api .

# Run the container
docker run -p 8080:8080 -p 8081:8081 hive-api
```

### File System Watcher

```bash
# Build the Docker image
docker build -f src/app/Watcher.Console.App/Dockerfile -t hive-watcher .

# Run the container with volume mounting
docker run -v /host/path:/app/watch hive-watcher --watch /app/watch
```

## 📡 API Documentation

When running in development mode, the API documentation is available through OpenAPI:
- Swagger UI: `https://localhost:5001/swagger` (when available)
- OpenAPI spec: `https://localhost:5001/openapi/v1.json`

## 🏛️ Project Structure

### Core Components

- **Domain**: Contains business entities, value objects, and domain services
- **Common**: Shared utilities and cross-cutting concerns
- **Features**: Feature-specific implementations following vertical slice architecture
- **Infrastructure**: Data persistence, external service integrations

### Applications

- **Hive.App**: ASP.NET Core Web API with authentication and authorization
- **Watcher.Console.App**: File system monitoring console application

## 🧪 Testing

The solution includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Domain.Tests/
dotnet test tests/Console.App.Tests/
```

### Test Structure

- **Unit Tests**: Domain logic and individual component testing
- **Integration Tests**: Component interaction testing
- **Mocking**: Uses Moq framework for test isolation

## 🔧 Configuration

### Web API Configuration

Configuration is managed through `appsettings.json` and `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Authentication

The API uses Windows Authentication with Negotiate scheme for secure access.

## 📁 File System Watcher Features

The console application provides:
- **Real-time monitoring**: Watches for file system changes
- **Configurable paths**: Specify custom directories to monitor
- **Event logging**: Comprehensive logging of file system events
- **Error handling**: Robust error handling and recovery

### Supported Events

- File creation
- File modification
- File deletion
- File renaming
- Directory changes

## 🛠️ Development

### Adding New Features

1. **Domain Layer**: Add entities and business logic
2. **Features Layer**: Implement feature-specific handlers
3. **Infrastructure**: Add data access or external service integrations
4. **Tests**: Write comprehensive tests for new functionality

### Code Style

The project follows standard C# coding conventions:
- Nullable reference types enabled
- Implicit usings enabled
- Clean code principles

## 📋 Available Commands

### File System Watcher Commands

```bash
# Watch a specific folder
dotnet run -- --watch /path/to/folder
dotnet run -- -w /path/to/folder

# Display help
dotnet run -- --help
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔍 Monitoring and Logging

The applications include comprehensive logging capabilities:
- Structured logging with Microsoft.Extensions.Logging
- Configurable log levels
- Console and file output options

## 🚨 Error Handling

Robust error handling is implemented throughout:
- Global exception handling in the Web API
- Graceful degradation in the file watcher
- Comprehensive error logging and reporting

---

For more information, please refer to the individual project documentation or contact the development team.
