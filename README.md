# 🎬 Hive - Self-Hosted Movie Management Platform

> A modern, microservices-based movie platform with automated media monitoring, user authentication, and a responsive web interface.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📖 Overview

**Hive** is an open-source, self-hosted movie management platform designed for media enthusiasts. It automatically monitors your movie directories, extracts metadata, and provides a beautiful web interface to browse and manage your collection. Built with modern .NET microservices architecture, it's fast, scalable, and easy to deploy.

### ✨ Key Features

- 🎥 **Automated Media Monitoring** - Watches your movie directories and automatically processes new files
- 🔐 **Secure Authentication** - JWT-based user authentication and authorization
- 🌐 **Modern Web UI** - Responsive interface for browsing and managing your collection
- 🎬 **Jellyfin Integration** - Seamlessly integrates with Jellyfin media server
- 🐳 **Docker Ready** - Easy deployment with Docker Compose
- 🏗️ **Clean Architecture** - Maintainable, testable, and scalable codebase

---

## 🏛️ System Architecture

Hive follows a **microservices architecture** with clean separation of concerns:

```mermaid
graph TB
    User[👤 User Browser]
    
    User -->|HTTP| Web[🌐 Web Interface<br/>Nginx + HTML/JS<br/>Port: 8000]
    
    Web -->|Auth Requests| IDM[🔐 Hive IDM<br/>Authentication API<br/>Port: 8082]
    Web -->|Movie Requests| App[🎬 Hive App<br/>Movie API<br/>Port: 8080]
    Web -->|Media Playback| Jellyfin[📺 Jellyfin<br/>Media Server<br/>Port: 8096]
    
    Watcher[👁️ Hive Watcher<br/>File System Monitor] -->|Notify Changes| App
    Watcher -->|Watch| Movies[(📁 Movie Files<br/>Directory)]
    
    App -->|Store Metadata| DB[(💾 Database)]
    IDM -->|User Data| UserDB[(🔐 User Database)]
    
    Jellyfin -->|Read Media| Movies
    
    style User fill:#e1f5ff
    style Web fill:#fff4e6
    style App fill:#f3e5f5
    style IDM fill:#e8f5e9
    style Jellyfin fill:#fce4ec
    style Watcher fill:#fff3e0
    style Movies fill:#f1f8e9
    style DB fill:#e0f2f1
    style UserDB fill:#e0f2f1
```

### 🔄 Data Flow

1. **User** accesses the web interface
2. **Web UI** authenticates via **Hive IDM** (gets JWT token)
3. **Web UI** fetches movie data from **Hive App**
4. **Watcher Service** monitors movie directory for changes
5. **Watcher Service** notifies **Hive App** of new/changed files
6. **Hive App** processes metadata and updates the collection
7. **Jellyfin** integration provides rich media playback

### 📊 Component Interaction

```mermaid
sequenceDiagram
    participant U as User
    participant W as Web UI
    participant I as Hive IDM
    participant A as Hive App
    participant FS as File Watcher
    participant J as Jellyfin
    
    U->>W: Access Dashboard
    W->>I: POST /auth/login
    I-->>W: JWT Token
    
    W->>A: GET /api/movies (+ JWT)
    A-->>W: Movie List
    W-->>U: Display Movies
    
    Note over FS: Monitoring Directory
    FS->>FS: Detect New File
    FS->>A: POST /api/movies/process
    A->>A: Extract Metadata
    A-->>FS: Processing Complete
    
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
│   │   ├── Hive.App/                 # Movie API (REST endpoints)
│   │   └── Watcher.Console.App/      # File system watcher service
│   │
│   ├── idm/
│   │   ├── Hive.Idm.Api/             # Authentication API
│   │   └── Hive.Idm.Infrastructure/  # User database & repositories
│   │
│   ├── web/                          # Frontend (HTML/CSS/JS)
│   │   ├── pages/                    # Web pages
│   │   └── assets/                   # Static resources
│   │
│   ├── domain/Domain/                # Business entities & logic
│   ├── features/Features/            # Use cases (CQRS handlers)
│   ├── Infrastructure/               # Data access & integrations
│   └── common/Common/                # Shared utilities
│
├── tests/                            # Unit & integration tests
├── docker-compose.yaml               # Multi-service orchestration
└── Makefile                          # Development commands
```

### 📦 Architecture Layers

| Layer | Responsibility | Examples |
|-------|---------------|----------|
| **Presentation** | User interfaces & APIs | Web UI, REST endpoints |
| **Application** | Use cases & orchestration | CQRS handlers, commands/queries |
| **Domain** | Business logic & rules | Movie entities, validation |
| **Infrastructure** | External concerns | Database, file I/O, APIs |

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

# 2. Set environment variables
export UID=$(id -u)
export GID=$(id -g)

# 3. Start all services
make up
# or: docker-compose up -d

# 4. Access the application
# Web UI:       http://localhost:8000
# Movie API:    http://localhost:8080
# Auth API:     http://localhost:8082
# Jellyfin:     http://localhost:8096
```

### Option 2: Local Development

```bash
# 1. Restore dependencies
dotnet restore

# 2. Build the solution
dotnet build

# 3. Run tests
dotnet test

# 4. Start services individually
# Terminal 1 - Movie API
cd src/app/Hive.App
dotnet run

# Terminal 2 - Auth API
cd src/idm/Hive.Idm.Api
dotnet run

# Terminal 3 - File Watcher
cd src/app/Watcher.Console.App
dotnet run -- --watch /path/to/movies
```

---

## ⚙️ Configuration

### Environment Variables

Create a `.env` file in the project root:

```bash
# User permissions (for Docker volumes)
UID=1000
GID=1000

# Movie directory (adjust to your path)
MOVIES_DIR=/home/user/Movies

# Jellyfin (optional)
JELLYFIN_BASE_URL=http://localhost:8096
JELLYFIN_ACCESS_TOKEN=your-api-key-here
```

### Docker Compose Services

| Service | Port | Description |
|---------|------|-------------|
| `hive-app` | 8080 | Movie API |
| `hive-idm` | 8082 | Authentication API |
| `hive-watcher` | - | File monitor (background) |
| `project-hive-web` | 8000 | Web interface |
| `jellyfin` | 8096 | Media server (optional) |

### Volume Mappings

```yaml
volumes:
  # Configuration & cache
  - jellyfin-config:/config
  - jellyfin-cache:/cache
  
  # Movie directory (read-only)
  - "/path/to/your/movies:/app/shared:ro"
  - "/path/to/your/movies:/media:ro"  # Jellyfin mount
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
make build         # Rebuild containers
make clean         # Remove containers
make status        # Show service status
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Common.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode (TDD)
dotnet watch test --project tests/Common.Tests/
```

### Adding New Features

1. **Domain Layer** - Add entities in `src/domain/Domain/Entities/`
2. **Features Layer** - Create handlers in `src/features/Features/`
3. **API Layer** - Add endpoints in `src/app/Hive.App/`
4. **Tests** - Write tests in corresponding `tests/` directory

---

## 🧪 Testing

The project includes comprehensive test coverage:

| Test Project | Coverage |
|--------------|----------|
| **Common.Tests** | Utilities, parsing, crypto |
| **Console.App.Tests** | File watcher logic |
| **Infrastructure.Tests** | Database, repositories |

**Testing Stack:**
- **xUnit** - Test framework
- **Moq** - Mocking
- **FluentAssertions** - Readable assertions

---

## 🐳 Docker Deployment

### Building Images

```bash
# Build all images
docker-compose build

# Build specific service
docker-compose build hive-app
```

### Production Deployment

```bash
# Start in production mode
ASPNETCORE_ENVIRONMENT=Production docker-compose up -d

# View logs
docker-compose logs -f

# Scale services (if needed)
docker-compose up -d --scale hive-app=3
```

### Health Checks

All services include health checks:

```bash
# Check service health
docker-compose ps

# Manual health check
curl http://localhost:8080/health
curl http://localhost:8082/health
```

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
- Ensure Docker builds succeed

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET 9.0](https://dotnet.microsoft.com/)
- [Jellyfin](https://jellyfin.org/) for media server integration
- [FastEndpoints](https://fast-endpoints.com/) for high-performance APIs
- [MediatR](https://github.com/jbogard/MediatR) for CQRS pattern

---

## 📞 Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/yourusername/project-hive/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/yourusername/project-hive/discussions)
- 📖 **Documentation**: Check individual project README files

---

**Built with ❤️ by the open-source community**
