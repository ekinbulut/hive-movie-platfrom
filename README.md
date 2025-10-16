# 🎬 Hive - Self-Hosted Movie Management Platform

> A modern, event-driven movie platform with automated media monitoring, user authentication, and a responsive web interface.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](https://www.docker.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-4.1-orange)](https://www.rabbitmq.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📖 Overview

**Hive** is an open-source, self-hosted movie management platform designed for media enthusiasts. It automatically monitors your movie directories, extracts metadata, and provides a beautiful web interface to browse and manage your collection. Built with modern .NET microservices architecture using event-driven patterns, it's fast, scalable, and easy to deploy.

### ✨ Key Features

- 🎥 **Automated Media Monitoring** - Real-time file system watching with event-driven processing
- 🔐 **Secure Authentication** - JWT-based user authentication with refresh tokens
- 🌐 **Modern Web UI** - Responsive interface for browsing and managing your collection
- 🎬 **Jellyfin Integration** - Seamlessly integrates with Jellyfin media server for playback
- 📨 **Event-Driven Architecture** - RabbitMQ-based messaging for scalable microservices
- 💾 **PostgreSQL Database** - Robust, unified data storage
- ⚡ **Redis Caching** - High-performance caching layer
- 🐳 **Docker Ready** - Easy deployment with Docker Compose
- 🏗️ **Clean Architecture** - Maintainable, testable, and scalable codebase

---

## 🏛️ System Architecture

Hive follows an **event-driven microservices architecture** with clean separation of concerns:

```mermaid
graph TB
    User[👤 User Browser]
    
    User -->|HTTP| Web[🌐 Web Interface<br/>Nginx + HTML/JS<br/>Port: 8000]
    
    Web -->|Auth Requests| IDM[🔐 Hive IDM API<br/>Authentication Service<br/>Port: 8082]
    Web -->|Movie Requests| App[🎬 Hive App API<br/>Movie Service<br/>Port: 8080]
    Web -->|Media Playback| Jellyfin[📺 Jellyfin<br/>Media Server<br/>Port: 8096]
    
    Watcher[👁️ Hive Watcher<br/>File System Monitor<br/>Background Service]
    Watcher -->|Watch Events| RabbitMQ[🐰 RabbitMQ<br/>Message Broker<br/>Port: 5672, 15672]
    Watcher -->|Scan Files| Movies[(📁 Movie Files<br/>Directory)]
    
    RabbitMQ -->|Path Changed Events| App
    RabbitMQ -->|File Found Events| Watcher
    
    App -->|Read/Write| DB[(💾 PostgreSQL<br/>Unified Database<br/>Port: 5432)]
    IDM -->|Read/Write| DB
    Watcher -->|Read/Write| DB
    
    App -->|Cache| Redis[⚡ Redis<br/>Cache Layer<br/>Port: 6379]
    
    Jellyfin -->|Read Media| Movies
    Watcher -->|Fetch Metadata| Jellyfin
    
    style User fill:#e1f5ff
    style Web fill:#fff4e6
    style App fill:#f3e5f5
    style IDM fill:#e8f5e9
    style Jellyfin fill:#fce4ec
    style Watcher fill:#fff3e0
    style Movies fill:#f1f8e9
    style DB fill:#e0f2f1
    style Redis fill:#ffe0b2
    style RabbitMQ fill:#ffccbc
```

### 🔄 Data Flow

1. **User** accesses the web interface
2. **Web UI** authenticates via **Hive IDM API** (gets JWT token)
3. **Web UI** fetches movie data from **Hive App API** (with JWT)
4. **Watcher Service** monitors movie directory for file changes
5. **Watcher Service** publishes events to **RabbitMQ**
6. **Hive App API** consumes events and processes metadata
7. **All services** share a unified **PostgreSQL** database
8. **Redis** caches frequently accessed data for performance
9. **Jellyfin** integration provides rich media playback experience

### 📊 Component Interaction

```mermaid
sequenceDiagram
    participant U as User
    participant W as Web UI
    participant I as Hive IDM
    participant A as Hive App
    participant MQ as RabbitMQ
    participant FS as File Watcher
    participant DB as PostgreSQL
    participant J as Jellyfin
    
    U->>W: Access Dashboard
    W->>I: POST /v1/api/auth/login
    I->>DB: Validate Credentials
    DB-->>I: User Data
    I-->>W: JWT Token + Refresh Token
    
    W->>A: GET /v1/api/movies (+ JWT)
    A->>DB: Query Movies
    DB-->>A: Movie List
    A-->>W: Movie Data
    W-->>U: Display Movies
    
    Note over FS: Monitoring Directory
    FS->>FS: Detect New File
    FS->>MQ: Publish WatchPathChangedEvent
    MQ->>A: Consume Event
    A->>J: Fetch Metadata
    J-->>A: Movie Information
    A->>DB: Store Movie Data
    
    U->>W: Play Movie
    W->>J: Stream Request
    J-->>U: Media Stream
```

---

## 🏗️ Project Structure

```
project-hive/
├── src/
│   ├── app/
│   │   ├── Hive.App/                 # Movie API (REST endpoints, FastEndpoints)
│   │   └── Watcher.Console.App/      # File system watcher service
│   │
│   ├── idm/
│   │   └── Hive.Idm.Api/             # Authentication API (JWT, User management)
│   │
│   ├── web/                          # Frontend (HTML/CSS/JS + Nginx)
│   │   ├── pages/                    # Web pages
│   │   ├── assets/                   # Static resources
│   │   └── Dockerfile                # Web container
│   │
│   ├── domain/Domain/                # Business entities & domain logic
│   │   ├── Entities/                 # Movie, User, Role, etc.
│   │   ├── Events/                   # Domain events
│   │   └── Interfaces/               # Repository contracts
│   │
│   ├── features/Features/            # Use cases (CQRS handlers)
│   │   ├── GetAllMovies/
│   │   ├── GetMoviesByFilter/
│   │   └── GetFilters/
│   │
│   ├── Infrastructure/Infrastructure/ # Data access & integrations
│   │   ├── Database/                 # EF Core, DbContext, Repositories
│   │   │   ├── Context/              # HiveDbContext
│   │   │   ├── Migrations/           # EF Core migrations
│   │   │   └── Repositories/         # Data access implementations
│   │   ├── Messaging/                # RabbitMQ (Rebus) configuration
│   │   ├── Caching/                  # Redis caching
│   │   └── Integration/              # External APIs (Jellyfin, TMDB)
│   │
│   └── common/Common/                # Shared utilities
│       ├── Parser/                   # Title parsing
│       └── Crypto/                   # Hashing helpers
│
├── tests/                            # Unit & integration tests
│   ├── Common.Tests/
│   ├── Console.App.Tests/
│   └── Infrastructure.Tests/
│
├── docker-compose.yaml               # Application services
├── docker-compose-infra.yaml         # Infrastructure services
└── Makefile                          # Development commands
```

### 📦 Architecture Layers

| Layer | Responsibility | Examples |
|-------|---------------|----------|
| **Presentation** | User interfaces & APIs | Web UI, REST endpoints (FastEndpoints) |
| **Application** | Use cases & orchestration | CQRS handlers, commands/queries (MediatR) |
| **Domain** | Business logic & rules | Movie entities, domain events, validation |
| **Infrastructure** | External concerns | Database (EF Core), messaging (Rebus), cache (Redis) |

---

## 🚀 Quick Start

### Prerequisites

- **Docker** or **Podman** (recommended)
- **Docker Compose** or **Podman Compose**
- **.NET 9.0 SDK** (only for local development)

### Option 1: Docker Compose (Recommended)

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/project-hive.git
cd project-hive

# 2. Set environment variables (for Jellyfin volume permissions)
export UID=$(id -u)
export GID=$(id -g)

# 3. Start infrastructure services (PostgreSQL, Redis, RabbitMQ, Jellyfin)
docker-compose -f docker-compose-infra.yaml up -d

# 4. Wait for PostgreSQL to be ready (check with docker-compose ps)
docker-compose -f docker-compose-infra.yaml ps

# 5. Run database migrations (IMPORTANT - First time setup)
# See "Database Migrations" section below

# 6. Start application services
docker-compose up -d

# 7. Access the application
# Web UI:       http://localhost:8000
# Movie API:    http://localhost:8080
# Auth API:     http://localhost:8082
# Jellyfin:     http://localhost:8096
# RabbitMQ UI:  http://localhost:15672 (user: admin, pass: admin)
```

### Option 2: Local Development

```bash
# 1. Restore dependencies
dotnet restore

# 2. Build the solution
dotnet build

# 3. Start infrastructure services
docker-compose -f docker-compose-infra.yaml up -d

# 4. Run database migrations
dotnet ef database update --project src/Infrastructure/Infrastructure --startup-project src/app/Hive.App

# 5. Run tests
dotnet test

# 6. Start services individually
# Terminal 1 - Movie API
cd src/app/Hive.App
dotnet run

# Terminal 2 - Auth API
cd src/idm/Hive.Idm.Api
dotnet run

# Terminal 3 - File Watcher
cd src/app/Watcher.Console.App
dotnet run
```

---

## 💾 Database Migrations

### Initial Setup (First Time Users)

Hive uses **Entity Framework Core** for database management. You must run migrations before using the application for the first time.

#### Method 1: Using Docker (Easiest)

```bash
# 1. Start PostgreSQL
docker-compose -f docker-compose-infra.yaml up -d postgres

# 2. Wait for PostgreSQL to be ready (15-30 seconds)
docker-compose -f docker-compose-infra.yaml logs -f postgres
# Look for: "database system is ready to accept connections"

# 3. Run migrations from the Hive.App container
docker-compose run --rm hive-app dotnet ef database update \
  --project /app/src/Infrastructure/Infrastructure/Infrastructure.csproj \
  --startup-project /app/src/app/Hive.App/Hive.App.csproj

# Alternative: Execute from your local machine (if .NET SDK installed)
dotnet ef database update \
  --project src/Infrastructure/Infrastructure/Infrastructure.csproj \
  --startup-project src/app/Hive.App/Hive.App.csproj
```

#### Method 2: Using .NET CLI (Local Development)

```bash
# 1. Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# 2. Update to the latest version
dotnet tool update --global dotnet-ef

# 3. Ensure PostgreSQL is running
docker-compose -f docker-compose-infra.yaml up -d postgres

# 4. Run migrations
cd /path/to/project-hive
dotnet ef database update \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App

# Expected output:
# Build started...
# Build succeeded.
# Applying migration '20251016091011_InitialCreate'.
# Done.
```

### Verify Database Setup

```bash
# Connect to PostgreSQL and check tables
docker exec -it postgres_db psql -U postgres -d hive_development

# Inside psql:
\dt              # List all tables
\d movies        # Describe movies table
\d users         # Describe users table
\q               # Quit
```

You should see these tables:
- `movies` - Movie metadata
- `users` - User accounts
- `roles` - User roles
- `user_roles` - User-role mappings
- `refresh_tokens` - JWT refresh tokens
- `audit_logs` - Audit trail
- `configurations` - Application settings
- `__EFMigrationsHistory` - EF Core migration tracking

### Creating New Migrations

When you modify entities in the `Domain` layer:

```bash
# 1. Create a new migration
dotnet ef migrations add YourMigrationName \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App \
  --output-dir Migrations

# 2. Review the generated migration files in:
# src/Infrastructure/Infrastructure/Migrations/

# 3. Apply the migration
dotnet ef database update \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App
```

### Troubleshooting Migrations

```bash
# Check current migration status
dotnet ef migrations list \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App

# Rollback to a specific migration
dotnet ef database update PreviousMigrationName \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App

# Remove the last migration (if not applied)
dotnet ef migrations remove \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App

# Generate SQL script without applying
dotnet ef migrations script \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App \
  --output migration.sql
```

---

## ⚙️ Configuration

### Environment Variables

Update your `appsettings.json` or set environment variables:

#### Hive.App (Movie API) - `src/app/Hive.App/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hive_development;Username=postgres;Password=postgres",
    "RabbitMq": "amqp://admin:admin@localhost:5672"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HiveIdm",
    "Audience": "HiveApi"
  }
}
```

#### Hive.Idm.Api (Auth API) - `src/idm/Hive.Idm.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hive_development;Username=postgres;Password=postgres",
    "RabbitMq": "amqp://admin:admin@localhost:5672"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HiveIdm",
    "Audience": "HiveApi"
  }
}
```

#### Watcher.Console.App - `src/app/Watcher.Console.App/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hive_development;Username=postgres;Password=postgres",
    "RabbitMq": "amqp://admin:admin@localhost:5672"
  },
  "JellyFin": {
    "BaseUrl": "http://localhost:8096",
    "ApiKey": "your-jellyfin-api-key"
  }
}
```

### Docker Compose Services

#### Application Services (`docker-compose.yaml`)

| Service | Port | Description |
|---------|------|-------------|
| `hive-app` | 8080 | Movie API (FastEndpoints) |
| `hive-idm` | 8082 | Authentication API |
| `hive-watcher` | - | File monitor (background service) |
| `project-hive-web` | 8000 | Web interface (Nginx) |

#### Infrastructure Services (`docker-compose-infra.yaml`)

| Service | Port | Description |
|---------|------|-------------|
| `postgres` | 5432 | PostgreSQL 15 database |
| `redis` | 6379 | Redis cache |
| `rabbitmq` | 5672, 15672 | RabbitMQ broker + Management UI |
| `jellyfin` | 8096 | Jellyfin media server |

### Volume Mappings

```yaml
# Infrastructure volumes
volumes:
  postgres_data:       # Database persistence
  redis_data:          # Cache persistence  
  rabbitmq_data:       # Message queue persistence
  jellyfin-config:     # Jellyfin configuration
  jellyfin-cache:      # Jellyfin cache

# Movie directory (update path in docker-compose-infra.yaml)
- "/home/user/shared/Plex/Shared Movies:/media:ro"  # Jellyfin mount
- "/home/user/shared:/mnt/host"                      # Watcher mount
```

---

## 🛠️ Development

### Make Commands

```bash
make help          # Show all available commands
make up            # Start all services
make down          # Stop all services
make restart       # Restart services
make logs          # View logs (all services)
make logs-app      # View Movie API logs
make logs-idm      # View Auth API logs
make logs-watcher  # View Watcher logs
make build         # Rebuild containers
make clean         # Remove containers
make status        # Show service status
make shell-app     # Enter Hive App container
make shell-idm     # Enter Hive IDM container
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Common.Tests/
dotnet test tests/Infrastructure.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode (TDD)
dotnet watch test --project tests/Common.Tests/
```

### Adding New Features

1. **Domain Layer** - Add entities in `src/domain/Domain/Entities/`
2. **Infrastructure Layer** - Create repositories in `src/Infrastructure/Infrastructure/Database/Repositories/`
3. **Features Layer** - Create CQRS handlers in `src/features/Features/`
4. **API Layer** - Add FastEndpoints in `src/app/Hive.App/`
5. **Database** - Create and apply EF Core migrations
6. **Tests** - Write tests in corresponding `tests/` directory

### Debugging

```bash
# View RabbitMQ queues and messages
# Open: http://localhost:15672 (admin/admin)

# View PostgreSQL data
docker exec -it postgres_db psql -U postgres -d hive_development

# View Redis cache
docker exec -it redis_cache redis-cli -a 181985
> KEYS *
> GET key_name

# Follow application logs
docker-compose logs -f hive-app hive-idm hive-watcher
```

---

## 🧪 Testing

The project includes comprehensive test coverage:

| Test Project | Coverage |
|--------------|----------|
| **Common.Tests** | Utilities, title parsing, crypto/hashing |
| **Console.App.Tests** | File watcher logic, event handling |
| **Infrastructure.Tests** | Database repositories, EF Core queries |

**Testing Stack:**
- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Readable assertions

---

## 🐳 Docker Deployment

### Building Images

```bash
# Build all application images
docker-compose build

# Build specific service
docker-compose build hive-app
docker-compose build hive-idm
docker-compose build hive-watcher
docker-compose build project-hive-web
```

### Production Deployment

```bash
# 1. Start infrastructure
docker-compose -f docker-compose-infra.yaml up -d

# 2. Run migrations
docker-compose run --rm hive-app dotnet ef database update \
  --project /app/src/Infrastructure/Infrastructure/Infrastructure.csproj \
  --startup-project /app/src/app/Hive.App/Hive.App.csproj

# 3. Start application in production mode
ASPNETCORE_ENVIRONMENT=Production docker-compose up -d

# 4. View logs
docker-compose logs -f

# 5. Check service health
docker-compose ps
```

### Health Checks

All services include health checks:

```bash
# Check container health status
docker-compose ps
docker-compose -f docker-compose-infra.yaml ps

# Manual health checks
curl http://localhost:8080/health     # Hive App (if configured)
curl http://localhost:8082/health     # Hive IDM (if configured)

# Database health
docker exec postgres_db pg_isready -U postgres

# RabbitMQ health
docker exec rabbitmq_broker rabbitmq-diagnostics ping

# Redis health
docker exec redis_cache redis-cli -a 181985 ping
```

---

## 🔧 Technology Stack

### Backend
- **.NET 9.0** - Runtime and framework
- **FastEndpoints** - High-performance REST APIs
- **MediatR** - CQRS pattern implementation
- **Entity Framework Core 9** - ORM and database migrations
- **Rebus** - Message bus abstraction

### Infrastructure
- **PostgreSQL 15** - Primary database
- **Redis 7** - Caching layer
- **RabbitMQ 4.1** - Message broker
- **Jellyfin** - Media server

### Frontend
- **Nginx** - Web server
- **Vanilla JavaScript** - Client-side logic
- **HTML5/CSS3** - User interface

---

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Development Guidelines

- Follow Clean Architecture principles
- Write tests for new features
- Update documentation
- Follow C# coding conventions
- Create EF Core migrations for database changes
- Ensure Docker builds succeed
- Test with both Docker and local development setups

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET 9.0](https://dotnet.microsoft.com/)
- [Jellyfin](https://jellyfin.org/) for media server integration
- [FastEndpoints](https://fast-endpoints.com/) for high-performance APIs
- [Rebus](https://github.com/rebus-org/Rebus) for messaging
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data access
- [RabbitMQ](https://www.rabbitmq.com/) for reliable messaging
- [PostgreSQL](https://www.postgresql.org/) for robust data storage

---

## 📞 Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/yourusername/project-hive/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/yourusername/project-hive/discussions)
- 📖 **Documentation**: Check individual project README files

---

**Built with ❤️ by the open-source community**
