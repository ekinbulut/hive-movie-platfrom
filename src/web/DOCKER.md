# 🐳 Docker Deployment Guide

This directory contains Docker configuration files for containerizing and deploying the Project Hive Web Application.

## 📋 Files Overview

- **`Dockerfile`** - Multi-stage Docker build for the web application
- **`docker-compose.yml`** - Development/local deployment setup
- **`docker-compose.prod.yml`** - Production deployment with API services
- **`.dockerignore`** - Files to exclude from Docker build context
- **`docker-deploy.sh`** - Automated build and deployment script

## 🚀 Quick Start

### Option 1: Using Docker Compose (Recommended)

```bash
# Development deployment
docker-compose up -d

# Production deployment (after configuring APIs)
docker-compose -f docker-compose.prod.yml up -d
```

### Option 2: Using Deployment Script

```bash
# Full deployment (build + run)
./docker-deploy.sh deploy

# Just build
./docker-deploy.sh build

# Just run
./docker-deploy.sh run

# View logs
./docker-deploy.sh logs

# Check status
./docker-deploy.sh status
```

### Option 3: Manual Docker Commands

```bash
# Build image
docker build -t project-hive-web .

# Run container
docker run -d \
  --name project-hive-web \
  -p 8080:80 \
  --restart unless-stopped \
  project-hive-web
```

## 🌐 Access Points

After deployment, the application will be available at:

- **Development**: http://localhost:8080
- **Production**: http://localhost:3000 (or your configured domain)

## 🔧 Configuration

### Environment Variables

For production deployment, create a `.env` file:

```env
# API Configuration
AUTH_DATABASE_URL=postgresql://user:password@localhost:5432/auth_db
MOVIES_DATABASE_URL=postgresql://user:password@localhost:5432/movies_db
JWT_SECRET=your-super-secret-jwt-key

# Movies Storage
MOVIES_PATH=/path/to/your/movie/files

# Database Configuration
DB_NAME=project_hive
DB_USER=project_hive_user
DB_PASSWORD=secure_password
```

### API Configuration

Update the API URLs in `/assets/js/dashboard-script.js` and `/assets/js/script.js`:

```javascript
const API_CONFIG = {
    AUTH_BASE_URL: 'http://auth-api:5188',     // For Docker internal networking
    MOVIES_BASE_URL: 'http://movies-api:5154', // For Docker internal networking
    // OR for external APIs:
    // AUTH_BASE_URL: 'https://your-auth-api.com',
    // MOVIES_BASE_URL: 'https://your-movies-api.com',
};
```

## 📊 Monitoring & Management

### Health Checks

The container includes built-in health checks:

```bash
# Check health status
docker inspect --format='{{.State.Health.Status}}' project-hive-web

# View health check logs
docker inspect --format='{{range .State.Health.Log}}{{.Output}}{{end}}' project-hive-web
```

### Logs

```bash
# View real-time logs
docker logs -f project-hive-web

# View last 100 lines
docker logs --tail 100 project-hive-web
```

### Container Management

```bash
# Stop container
docker stop project-hive-web

# Start container
docker start project-hive-web

# Restart container
docker restart project-hive-web

# Remove container
docker rm -f project-hive-web
```

## 🔧 Nginx Configuration

The Docker image uses nginx with optimized configuration including:

- ✅ **Gzip compression** for faster loading
- ✅ **Security headers** for enhanced security
- ✅ **Asset caching** for better performance
- ✅ **SPA routing** support for client-side navigation
- ✅ **API proxy** ready (commented out, uncomment as needed)

### Custom Nginx Configuration

To use a custom nginx configuration, mount it as a volume:

```bash
docker run -d \
  --name project-hive-web \
  -p 8080:80 \
  -v ./custom-nginx.conf:/etc/nginx/conf.d/default.conf:ro \
  project-hive-web
```

## 🔐 Security Considerations

### Production Security

For production deployments:

1. **Use HTTPS**: Configure SSL/TLS certificates
2. **Environment Variables**: Store secrets in environment variables, not in code
3. **Network Security**: Use Docker networks to isolate containers
4. **Regular Updates**: Keep base images updated
5. **Resource Limits**: Set memory and CPU limits

Example with resource limits:

```yaml
services:
  project-hive-web:
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'
```

### SSL Configuration

For HTTPS, you can use a reverse proxy like Traefik or nginx:

```yaml
# Add to docker-compose.yml
labels:
  - "traefik.enable=true"
  - "traefik.http.routers.project-hive.rule=Host(\`your-domain.com\`)"
  - "traefik.http.routers.project-hive.tls.certresolver=letsencrypt"
```

## 🐛 Troubleshooting

### Common Issues

1. **Port Already in Use**
   ```bash
   # Find process using port
   lsof -i :8080
   # Kill process or use different port
   docker run -p 8081:80 project-hive-web
   ```

2. **Container Won't Start**
   ```bash
   # Check logs for errors
   docker logs project-hive-web
   # Check if image built correctly
   docker images | grep project-hive-web
   ```

3. **API Connection Issues**
   - Ensure API services are running
   - Check network connectivity between containers
   - Verify API URLs in configuration

4. **Permission Issues**
   ```bash
   # Fix file permissions
   sudo chown -R $USER:$USER .
   chmod +x docker-deploy.sh
   ```

### Debug Mode

Run container with debug output:

```bash
docker run -it --rm \
  --name project-hive-web-debug \
  -p 8080:80 \
  project-hive-web \
  nginx -g "daemon off; error_log /dev/stdout debug;"
```

## 📈 Performance Optimization

### Multi-stage Builds

The Dockerfile is optimized for production with:
- Minimal Alpine Linux base image
- Gzip compression enabled
- Static asset caching
- Health checks included

### Resource Optimization

```yaml
# Add to docker-compose.yml for resource limits
deploy:
  resources:
    limits:
      memory: 256M
      cpus: '0.25'
```

## 🔄 CI/CD Integration

Example GitHub Actions workflow:

```yaml
name: Build and Deploy
on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build and Deploy
        run: |
          docker build -t project-hive-web .
          docker run -d -p 8080:80 project-hive-web
```

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [Project Hive API Documentation](../api/README.md)

## 🆘 Support

If you encounter issues:
1. Check the logs: `docker logs project-hive-web`
2. Verify configuration files
3. Check Docker and system requirements
4. Review this documentation

---

**Happy containerizing! 🐳✨**