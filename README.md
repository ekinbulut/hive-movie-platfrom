# 🎬 Hive - Self-Hosted Movie Management Platform

> A modern, event-driven movie platform with automated media monitoring, user authentication, and a responsive web interface.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](https://www.docker.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-4.1-orange)](https://www.rabbitmq.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📖 Overview

**Hive** is an open-source, self-hosted movie management platform designed for media enthusiasts. It automatically monitors your movie directories, extracts metadata, and provides a beautiful web interface to browse and manage your collection.

### ✨ Key Features

- 🎥 **Automated Media Monitoring** - Real-time file system watching with event-driven processing
- 🔐 **Secure Authentication** - JWT-based user authentication with refresh tokens
- 🌐 **Modern Web UI** - Responsive interface for browsing and managing your collection
- 🎬 **Jellyfin Integration** - Seamlessly integrates with Jellyfin media server for playback
- 📨 **Event-Driven Architecture** - RabbitMQ-based messaging for scalable microservices
- 💾 **PostgreSQL Database** - Robust, unified data storage
- ⚡ **Redis Caching** - High-performance caching layer
- 🐳 **Docker/Podman Ready** - Easy deployment with Docker or Podman Compose

---

## 🏗️ Project Structure

```
project-hive/
├── src/
│   ├── app/
│   │   ├── Hive.App/                    # Movie API
│   │   ├── Watcher.Console.App/         # File system watcher
│   │   └── MetaScraper.App/             # Metadata scraper
│   ├── idm/
│   │   └── Hive.Idm.Api/                # Authentication API
│   ├── web/                             # Frontend (HTML/CSS/JS)
│   ├── domain/Domain/                   # Business entities & domain logic
│   ├── features/Features/               # CQRS handlers (use cases)
│   ├── Infrastructure/Infrastructure/   # Data access & integrations
│   └── common/Common/                   # Shared utilities
├── tests/                               # Unit & integration tests
├── docker-compose.yaml                  # Application services
├── docker-compose-infra.yaml            # Infrastructure services
└── Makefile                             # Development commands
```

---

## 🚀 Quick Start

### Prerequisites

- **Docker** or **Podman**
- **Docker Compose** or **Podman Compose**
- **.NET 9.0 SDK** (only for local development)

### Environment Setup

Create a `.env` file in the project root for GitHub package authentication:

```bash
# Copy the sample environment file
cp .env_sample .env

# Edit .env and add your GitHub credentials
GITHUB_USERNAME=your-github-username
GITHUB_TOKEN=your-github-personal-access-token
```

> **Note:** GitHub credentials are required for building Docker images if your project uses private NuGet packages from GitHub Package Registry. If you don't use private packages, you can leave these empty.

### Using Docker/Podman Compose

> **Note:** Replace `docker-compose` with `podman-compose` if you're using Podman.

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/project-hive.git
cd project-hive

# 2. Set environment variables (for Jellyfin volume permissions)
export UID=$(id -u)
export GID=$(id -g)

# 3. Start infrastructure services (PostgreSQL, Redis, RabbitMQ, Jellyfin)
docker-compose -f docker-compose-infra.yaml up -d
# Or with Podman:
# podman-compose -f docker-compose-infra.yaml up -d

# 4. Run database migrations (first time setup)
docker-compose run --rm hive-app dotnet ef database update \
  --project /app/src/Infrastructure/Infrastructure/Infrastructure.csproj \
  --startup-project /app/src/app/Hive.App/Hive.App.csproj

# 5. Start application services
docker-compose up -d
# Or with Podman:
# podman-compose up -d

# 6. Access the application
# Web UI:       http://localhost:8000
# Movie API:    http://localhost:8080
# Auth API:     http://localhost:8082
# Jellyfin:     http://localhost:8096
# RabbitMQ UI:  http://localhost:15672 (admin/admin)
```

### Local Development (Without Containers)

```bash
# 1. Restore dependencies
dotnet restore

# 2. Start infrastructure services
docker-compose -f docker-compose-infra.yaml up -d

# 3. Run database migrations
dotnet ef database update \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App

# 4. Start services individually
# Terminal 1 - Movie API
cd src/app/Hive.App && dotnet run

# Terminal 2 - Auth API
cd src/idm/Hive.Idm.Api && dotnet run

# Terminal 3 - File Watcher
cd src/app/Watcher.Console.App && dotnet run
```

---

## 💾 Database Setup

### First Time Setup

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Start PostgreSQL
docker-compose -f docker-compose-infra.yaml up -d postgres

# Run migrations
dotnet ef database update \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App
```

### Verify Database

```bash
# Connect to PostgreSQL
docker exec -it postgres_db psql -U postgres -d hive_development

# List tables
\dt

# Expected tables: movies, users, roles, user_roles, refresh_tokens, audit_logs
```

### Create New Migration

```bash
dotnet ef migrations add YourMigrationName \
  --project src/Infrastructure/Infrastructure \
  --startup-project src/app/Hive.App \
  --output-dir Migrations
```

---

## ⚙️ Configuration

### Key Configuration Files

| File | Purpose |
|------|---------|
| `src/app/Hive.App/appsettings.json` | Movie API settings |
| `src/idm/Hive.Idm.Api/appsettings.json` | Auth API settings |
| `src/app/Watcher.Console.App/appsettings.json` | File watcher settings |

### Example Configuration

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
  },
  "JellyFin": {
    "BaseUrl": "http://localhost:8096",
    "ApiKey": "your-jellyfin-api-key"
  }
}
```

### Services & Ports

| Service | Port | Description |
|---------|------|-------------|
| Web UI | 8000 | Frontend interface |
| Hive App | 8080 | Movie API |
| Hive IDM | 8082 | Authentication API |
| Jellyfin | 8096 | Media server |
| RabbitMQ | 5672, 15672 | Message broker + UI |
| PostgreSQL | 5432 | Database |
| Redis | 6379 | Cache |

---

## 🛠️ Development

### Make Commands

```bash
make help          # Show all available commands
make up            # Start all services
make down          # Stop all services
make logs          # View logs
make build         # Rebuild containers
make status        # Show service status
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Common.Tests/

# Watch mode
dotnet watch test --project tests/Common.Tests/
```

---

## 🐳 Docker & Podman Commands

### Build Images

```bash
# Docker
docker-compose build

# Podman
podman-compose build
```

### View Logs

```bash
# Docker
docker-compose logs -f hive-app

# Podman
podman-compose logs -f hive-app
```

### Container Management

```bash
# Docker
docker-compose ps
docker-compose stop
docker-compose restart

# Podman
podman-compose ps
podman-compose stop
podman-compose restart
```

---

## 🧪 Testing

Test coverage includes:

- **Common.Tests** - Utilities, title parsing, crypto
- **Console.App.Tests** - File watcher logic
- **Infrastructure.Tests** - Database repositories

**Stack:** xUnit, Moq, FluentAssertions

---

## 🔧 Technology Stack

### Backend
- .NET 9.0
- FastEndpoints (REST APIs)
- base-mediatr (CQRS)
- Entity Framework Core 9
- base-transport (Messaging)

### Infrastructure
- PostgreSQL 15
- Redis 7
- RabbitMQ 4.1
- Jellyfin

### Frontend
- Nginx
- HTML5/CSS3/JavaScript

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 📞 Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/yourusername/project-hive/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/yourusername/project-hive/discussions)

---

**Built with ❤️ by the open-source community**
