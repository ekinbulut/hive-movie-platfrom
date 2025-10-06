# Docker Environment Configuration

This document explains how the Hive Movie Platform handles API endpoints in both development and Docker environments.

## Architecture

The platform uses **environment-aware configuration** that automatically detects whether it's running in development or Docker environment:

### Development Environment
- **Web UI**: `http://localhost:8000`
- **Movies API**: `http://localhost:8080`
- **Identity API**: `http://localhost:8082`

### Docker Environment
- **Web UI**: `http://localhost:8000` (external access)
- **Movies API**: `http://hive-app:8080` (internal Docker network)
- **Identity API**: `http://hive-idm:8082` (internal Docker network)

## How It Works

### 1. Environment Detection
The JavaScript configuration automatically detects the environment:

```javascript
const API_CONFIG = {
    AUTH_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:8082'        // Development
        : 'http://hive-idm:8082',       // Docker
    MOVIES_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:8080'        // Development  
        : 'http://hive-app:8080'        // Docker
};
```

### 2. Docker Environment Variables
The web container receives environment variables and injects them into the runtime:

```yaml
environment:
  - AUTH_BASE_URL=http://hive-idm:8082
  - MOVIES_BASE_URL=http://hive-app:8080
```

### 3. Runtime Configuration
- A template file `env-config.js.template` contains placeholders
- At container startup, `entrypoint.sh` replaces placeholders with actual environment variables
- The generated `env-config.js` is loaded by HTML pages

## Quick Start

### Option 1: Using the Start Script (Recommended)
```bash
./start.sh
```

### Option 2: Manual Docker Compose
```bash
# Build and start all services
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## File Structure

```
src/web/
├── env-config.js              # Development fallback
├── env-config.js.template     # Docker template
├── entrypoint.sh             # Docker startup script
├── Dockerfile                # Updated with env support
├── assets/js/
│   ├── script.js             # Login page (env-aware)
│   └── dashboard-script.js   # Dashboard page (env-aware)
└── pages/
    ├── login.html            # Includes env-config.js
    └── dashboard.html        # Includes env-config.js
```

## Environment Variables

| Variable | Development | Docker | Description |
|----------|------------|--------|-------------|
| `AUTH_BASE_URL` | `http://localhost:8082` | `http://hive-idm:8082` | Identity service URL |
| `MOVIES_BASE_URL` | `http://localhost:8080` | `http://hive-app:8080` | Movies service URL |

## Networking

### Docker Network: `hive-network`
- **hive-app**: Movies API service
- **hive-idm**: Identity service  
- **hive-watcher**: File watcher service
- **project-hive-web**: Web UI (nginx)

### Port Mapping
- `8000:80` - Web UI
- `8080:8080` - Movies API
- `8082:8082` - Identity API

## User Experience Features

✅ **Automatic Environment Detection** - No manual configuration needed
✅ **Seamless Development** - Works locally without Docker
✅ **Container-Ready** - Works in Docker with proper service names
✅ **Error Handling** - Clear error messages for connectivity issues
✅ **Health Checks** - All services have health monitoring
✅ **Easy Startup** - Single command to start entire platform

## Troubleshooting

### API Connection Issues
1. Check if all containers are running: `docker-compose ps`
2. View service logs: `docker-compose logs [service-name]`
3. Test network connectivity: `docker-compose exec project-hive-web ping hive-app`

### Environment Configuration Issues
1. Check if `env-config.js` exists in the web container:
   ```bash
   docker-compose exec project-hive-web cat /usr/share/nginx/html/env-config.js
   ```
2. Verify environment variables:
   ```bash
   docker-compose exec project-hive-web env | grep -E "(AUTH|MOVIES)_BASE_URL"
   ```

### Shared Directory Issues
1. Ensure `./shared` directory exists and has proper permissions
2. Check if movies are accessible: `ls -la ./shared`
3. Verify watcher service: `docker-compose logs hive-watcher`