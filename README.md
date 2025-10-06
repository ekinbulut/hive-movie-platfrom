# 🎬 Hive Movie Platform

A comprehensive, modular .NET 9.0 movie platform solution featuring microservices architecture, real-time file system monitoring, identity management, and a modern web interface.

## � Overview

Hive is an enterprise-grade movie platform ecosystem built using clean architecture and microservices principles, featuring:

- **🎯 Core Movie API**: RESTful API for movie platform operations and content management
- **🔐 Identity Management (IDM)**: Dedicated authentication and authorization service with JWT support
- **👁️ File System Watcher**: Real-time console application for monitoring and processing media files
- **🌐 Modern Web Interface**: Responsive SPA with authentication, dashboard, and movie management
- **🏗️ Clean Architecture**: Domain-driven design with clear separation of concerns
- **🐳 Containerized Deployment**: Full Docker/Podman support with orchestration
- **⚡ High Performance**: Optimized for scalability and performance

## 🏗️ Architecture & Project Structure

The solution follows Domain-Driven Design (DDD), Clean Architecture, and Microservices patterns:

```
📦 Hive Movie Platform
├── 🚀 Applications
│   ├── src/app/Hive.App/           # Core Movie API (Port: 8080)
│   ├── src/app/Watcher.Console.App/ # File System Monitor
│   └── src/idm/Hive.Idm.Api/       # Identity Management API (Port: 8082)
│
├── 🧱 Core Layers
│   ├── src/domain/Domain/           # Business entities & domain logic
│   ├── src/common/Common/           # Shared utilities & cross-cutting concerns
│   ├── src/features/Features/       # Feature implementations (vertical slices)
│   └── src/Infrastructure/          # Data access & external integrations
│
├── 🌐 Web Interface
│   ├── src/web/pages/              # HTML pages (login, dashboard)
│   ├── src/web/assets/             # Static assets (CSS, JS)
│   └── src/web/tools/              # Development & testing tools
│
├── 🧪 Testing
│   ├── tests/Common.Tests/         # Common utilities tests
│   ├── tests/Console.App.Tests/    # File watcher tests
│   └── tests/Infrastructure.Tests/ # Infrastructure layer tests
│
└── 🐳 Deployment
    ├── docker-compose.yaml         # Multi-service orchestration
    ├── Makefile                   # Development commands
    └── Individual Dockerfiles      # Per-service containerization
```

## 🚀 Quick Start

### Prerequisites

- **[.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** - Core development framework
- **[Docker](https://www.docker.com/) or [Podman](https://podman.io/)** - Container runtime (recommended)
- **Modern Web Browser** - For web interface access

### 🐳 Containerized Deployment (Recommended)

The fastest way to get started is using the provided Makefile with Docker/Podman:

```bash
# Clone the repository
git clone <repository-url>
cd hive-movie-platfrom

# Show available commands
make help

# Start all services (builds images if needed)
make up

# View service status and logs
make status
make logs

# Stop all services
make down
```

**🌐 Access Points:**
- **Main Movie API**: http://localhost:8080
- **Identity Management**: http://localhost:8082  
- **Web Interface**: Served via nginx (check docker-compose.yaml for port)

### 🛠️ Development Setup

For local development without containers:

```bash
# Restore all dependencies
dotnet restore

# Build the entire solution
dotnet build

# Run comprehensive tests
dotnet test
```

### 🎯 Running Individual Services

#### Core Movie API
```bash
cd src/app/Hive.App
dotnet run
# Available at: http://localhost:8080
```

#### Identity Management API
```bash
cd src/idm/Hive.Idm.Api  
dotnet run
# Available at: http://localhost:8082
```

#### File System Watcher
```bash
cd src/app/Watcher.Console.App

# Watch a specific directory
dotnet run -- --watch /path/to/movies

# With volume mapping in Docker
docker run -v /host/movies:/app/shared hive-watcher --watch /app/shared
```

## 🐳 Container Architecture

The platform uses a multi-service architecture with the following containers:

### 🎯 Core Services
- **hive-app**: Main movie platform API (Port: 8080)
- **hive-idm**: Identity management service (Port: 8082) 
- **hive-watcher**: File system monitoring service

### 🔧 Advanced Container Operations

```bash
# Build specific service
make build
podman-compose build hive-app

# View real-time logs for specific service
make logs-app
make logs-idm

# Restart services after changes
make restart

# Complete cleanup (removes containers and images)
make clean-all

# Health check status
make status
```

### 📁 Volume Mappings
- **Media Directory**: `/home/user/shared/Plex/Shared Movies` → `/app/shared`
- **Configuration**: Auto-generated from environment variables

## 📡 API Documentation & Endpoints

### 🎯 Core Movie API (Port: 8080)
- **OpenAPI/Swagger**: `http://localhost:8080/swagger` (Development mode)
- **Health Check**: `http://localhost:8080/health`
- **Movies Endpoint**: `GET /api/movies` - Retrieve movie collection
- **OpenAPI Spec**: `http://localhost:8080/openapi/v1.json`

### 🔐 Identity Management API (Port: 8082)
- **Authentication**: `POST /auth/login` - JWT token generation
- **Token Validation**: `POST /auth/validate` - Verify JWT tokens
- **User Management**: Various endpoints for user operations
- **Swagger UI**: `http://localhost:8082/swagger`

### 🌐 Web Interface Features
- **🔐 JWT Authentication**: Secure login with automatic token management
- **📊 Movie Dashboard**: Browse, search, and manage movie collections
- **📱 Responsive Design**: Mobile-friendly interface
- **⚡ Real-time Updates**: Live data synchronization with APIs

## 🏛️ Detailed Component Overview

### 🎯 Core Applications

| Service | Port | Description | Key Features |
|---------|------|-------------|--------------|
| **Hive.App** | 8080 | Main Movie Platform API | RESTful endpoints, movie management, file processing |
| **Hive.Idm.Api** | 8082 | Identity Management Service | JWT authentication, user management, CORS support |
| **Watcher.Console.App** | - | File System Monitor | Real-time file detection, metadata extraction, automated processing |

### 🧱 Architecture Layers

| Layer | Purpose | Key Components |
|-------|---------|----------------|
| **Domain** | Business Logic | Entities, Value Objects, Domain Services, Business Rules |
| **Features** | Use Cases | CQRS Handlers, Vertical Slice Architecture, Feature-specific Logic |
| **Infrastructure** | External Concerns | Database Access, File I/O, Message Queuing, Caching |
| **Common** | Shared Utilities | Cryptography, Parsing, Cross-cutting Concerns |

### 🌐 Web Interface Components

- **🔐 Authentication Pages**: Secure login with JWT integration
- **📊 Dashboard Interface**: Movie collection management and statistics  
- **🛠️ Development Tools**: API testing utilities and debugging interfaces
- **📱 Responsive Design**: Mobile-first CSS with modern UI patterns

## 🧪 Testing Strategy

### 📊 Test Coverage Overview

The solution implements a comprehensive testing strategy across multiple layers:

| Test Project | Scope | Coverage |
|--------------|-------|----------|
| **Common.Tests** | Utilities & Helpers | Cryptography, Parsing, Shared Logic |
| **Console.App.Tests** | File System Watcher | File Detection, Event Handling, Service Logic |
| **Infrastructure.Tests** | Data & External Services | Repository Pattern, Database Operations |

### 🚀 Running Tests

```bash
# Execute all tests with detailed output
dotnet test --verbosity normal

# Generate code coverage reports
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific test project
dotnet test tests/Common.Tests/ --logger "console;verbosity=detailed"
dotnet test tests/Console.App.Tests/ --filter "Category=Unit"
dotnet test tests/Infrastructure.Tests/ --filter "FullyQualifiedName~Repository"

# Run tests in watch mode for TDD
dotnet watch test --project tests/Common.Tests/
```

### 🔧 Testing Technologies

- **xUnit**: Primary testing framework with powerful assertions
- **Moq**: Mocking framework for dependency isolation  
- **FluentAssertions**: Enhanced readability for test assertions
- **TestContainers**: Integration testing with real database instances

## ⚙️ Configuration Management

### 🔐 Authentication & Security

The platform implements a modern JWT-based authentication system:

**Identity Management Service Features:**
- **JWT Token Generation**: Secure token creation with configurable expiration
- **CORS Support**: Cross-origin requests for web interface integration
- **Entity Framework**: Database-backed user management
- **FastEndpoints**: High-performance API endpoints

### 🛠️ Environment Configuration

Each service supports environment-specific configuration:

```bash
# Development Environment Variables
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080  # Hive.App
ASPNETCORE_URLS=http://+:8082  # Hive.Idm.Api

# Database Configuration (Hive.Idm.Infrastructure)
# Configured via appsettings.json with connection strings

# File Watcher Configuration
# Command-line arguments: --watch /path/to/monitor
```

### 📁 Configuration Files

| Service | Configuration Files | Purpose |
|---------|-------------------|---------|
| **Hive.App** | `appsettings.json`, `appsettings.Development.json` | API settings, logging, allowed hosts |
| **Hive.Idm.Api** | `appsettings.json`, `appsettings.Development.json` | JWT config, database connection, CORS |
| **Watcher.Console.App** | `appsettings.json` | Logging configuration, service settings |

## �️ File System Watcher Capabilities

### 🚀 Advanced Monitoring Features

The Watcher service provides enterprise-grade file system monitoring:

| Feature | Description | Implementation |
|---------|-------------|----------------|
| **Real-time Detection** | Instant notification of file system changes | `FileSystemWatcher` with event-driven architecture |
| **Metadata Extraction** | Automatic movie file analysis | Custom parsers for file names and properties |
| **Health Monitoring** | Container health checks and status reporting | Docker health check integration |
| **Volume Integration** | Seamless Docker volume mounting | Configured for `/app/shared` monitoring |
| **Error Recovery** | Robust exception handling and service restart | Comprehensive logging and graceful degradation |

### 📊 Supported File Operations

- ✅ **File Creation**: New movie files detected automatically
- ✅ **File Modification**: Changes to existing files tracked  
- ✅ **File Deletion**: Cleanup operations logged and processed
- ✅ **File Renaming**: Name changes captured with before/after states
- ✅ **Directory Operations**: Folder structure changes monitored
- ✅ **Batch Processing**: Multiple file operations handled efficiently

### 🔧 Service Architecture

```bash
# Service Dependencies
hive-watcher:
  depends_on:
    - hive-app      # Core API for data submission
    - hive-idm      # Authentication for secure operations
  
# Volume Configuration
volumes:
  - "/home/user/shared/Plex/Shared Movies:/app/shared"

# Health Monitoring
healthcheck:
  test: ["CMD", "test", "-d", "/app/shared"]
  interval: 30s
```

## 🛠️ Development Workflow

### 🚀 Adding New Features

Follow the clean architecture approach for new feature development:

1. **📋 Domain Layer** 
   - Define entities in `src/domain/Domain/Entities/`
   - Implement business rules and domain services
   - Create value objects for complex data types

2. **⚡ Features Layer**
   - Use CQRS pattern in `src/features/Features/`
   - Implement command/query handlers
   - Follow vertical slice architecture

3. **🔧 Infrastructure Layer**
   - Add repositories in `src/Infrastructure/Database/`
   - Implement external service integrations
   - Configure caching and messaging

4. **🧪 Testing Strategy**
   - Write unit tests for domain logic
   - Create integration tests for repositories
   - Add end-to-end tests for complete workflows

### 📝 Code Standards & Best Practices

The project enforces modern C# development standards:

```csharp
// ✅ Enabled Features
- Nullable reference types
- Implicit usings  
- File-scoped namespaces
- Record types for DTOs
- Minimal APIs where appropriate

// ✅ Architecture Patterns
- Clean Architecture boundaries
- CQRS with MediatR
- Repository pattern
- Dependency injection
- FastEndpoints for high-performance APIs
```

### 🐳 Development Environment Setup

```bash
# Start development environment
make up

# Monitor logs during development  
make logs

# Rebuild after code changes
make build && make restart

# Run tests continuously during development
dotnet watch test --project tests/Common.Tests/
```

## 📋 Command Reference

### 🐳 Container Management (Makefile)

| Command | Description | Usage |
|---------|-------------|-------|
| `make help` | Show all available commands | Default command |
| `make up` | Start all services | Builds images if needed |
| `make down` | Stop all services | Graceful shutdown |
| `make restart` | Restart all services | `down` + `up` |
| `make build` | Build all container images | Force rebuild |
| `make logs` | Show logs for all services | Last 20 lines |
| `make logs-app` | Show Hive App logs | Real-time follow |
| `make logs-idm` | Show IDM service logs | Real-time follow |
| `make status` | Show container status | Health and ports |
| `make clean` | Remove containers | Keeps images |
| `make clean-all` | Complete cleanup | Removes everything |

### 🔧 Development Commands

```bash
# .NET Solution Management
dotnet restore                    # Restore all dependencies
dotnet build                     # Build entire solution  
dotnet test                      # Run all tests
dotnet run --project src/app/Hive.App/    # Run Core API

# File System Watcher
cd src/app/Watcher.Console.App/
dotnet run -- --watch /movies   # Monitor directory
dotnet run -- --help           # Show watcher options

# Identity Management API
cd src/idm/Hive.Idm.Api/
dotnet run                      # Start IDM service
```

### 🌐 Service URLs

- **🎯 Core Movie API**: `http://localhost:8080`
- **🔐 Identity Management**: `http://localhost:8082` 
- **📚 API Documentation**: `http://localhost:8080/swagger` & `http://localhost:8082/swagger`

## 🔍 Monitoring & Observability

### 📊 Logging Strategy

The platform implements comprehensive logging across all services:

| Service | Logging Features | Configuration |
|---------|------------------|---------------|
| **All Services** | Structured logging with `Microsoft.Extensions.Logging` | `appsettings.json` |
| **Hive.App** | Request/response logging, performance metrics | Console + file output |
| **Hive.Idm.Api** | Authentication events, security logging | JWT token lifecycle |
| **Watcher Service** | File system events, error tracking | Real-time console output |

### 🚨 Error Handling & Resilience

- **🌐 Global Exception Handling**: Centralized error processing in Web APIs
- **🔄 Automatic Recovery**: File watcher service graceful restart capabilities  
- **� Comprehensive Logging**: All errors logged with context and stack traces
- **🏥 Health Checks**: Container health monitoring with automatic restarts
- **⚡ Performance Monitoring**: Request timing and resource usage tracking

### � Production Readiness Features

- **🐳 Container Health Checks**: Built-in monitoring for all services
- **🔐 Security Headers**: CORS, authentication, and authorization policies
- **⚡ Performance Optimization**: Efficient database queries and caching strategies
- **📊 Metrics Collection**: Ready for integration with monitoring solutions

## 🤝 Contributing

We welcome contributions! Please follow our development workflow:

1. **🍴 Fork the repository** and create your feature branch
   ```bash
   git checkout -b feature/amazing-new-feature
   ```

2. **🧪 Ensure all tests pass** before submitting
   ```bash
   dotnet test
   make build  # Verify container builds
   ```

3. **📝 Follow code standards** and add appropriate tests
4. **📤 Submit a Pull Request** with a clear description of changes

### 🎯 Development Guidelines

- **Clean Architecture**: Maintain clear separation of concerns
- **Test Coverage**: Ensure new features include comprehensive tests  
- **Documentation**: Update README and inline documentation
- **Container Compatibility**: Verify Docker/Podman builds succeed

## 📄 License & Support

- **📋 License**: MIT License - see LICENSE file for details
- **🐛 Issues**: Use GitHub Issues for bug reports and feature requests  
- **💬 Discussions**: GitHub Discussions for questions and community support
- **📚 Documentation**: Comprehensive README files in each project directory

---

## 🚀 Quick Links

- **🎯 [Core API Documentation](src/app/Hive.App/)** - Movie platform API details
- **🔐 [Identity Management](src/idm/Hive.Idm.Api/)** - Authentication service info  
- **👁️ [File Watcher Service](src/app/Watcher.Console.App/)** - File monitoring details
- **🌐 [Web Interface](src/web/)** - Frontend application documentation
- **🧪 [Testing Strategy](tests/)** - Comprehensive test suite information

**Built with ❤️ using .NET 9.0, Clean Architecture, and Modern DevOps Practices**
