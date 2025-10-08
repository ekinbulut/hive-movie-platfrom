# Use official nginx image as base
FROM nginx:alpine

# Set maintainer label
LABEL maintainer="project-hive"
LABEL description="Project Hive Web Application - Movie Collection Dashboard"

# Install gettext for envsubst command
RUN apk add --no-cache gettext

# Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*

# Copy the web application files to nginx html directory (exclude Docker/config files)
COPY src/web/index.html /usr/share/nginx/html/
COPY src/web/assets/ /usr/share/nginx/html/assets/
COPY src/web/pages/ /usr/share/nginx/html/pages/
COPY src/web/tools/ /usr/share/nginx/html/tools/
COPY src/web/env-config.js.template /usr/share/nginx/html/
COPY src/web/entrypoint.sh /usr/local/bin/

# Copy nginx configuration
COPY src/web/nginx.conf /etc/nginx/conf.d/default.conf

# Set proper permissions
RUN chmod -R 755 /usr/share/nginx/html
RUN chown -R nginx:nginx /usr/share/nginx/html
RUN chmod +x /usr/local/bin/entrypoint.sh

# Set default environment variables
ENV AUTH_BASE_URL=http://hive-idm:8082
ENV MOVIES_BASE_URL=http://hive-app:8080
ENV JELLYFIN_BASE_URL=http://192.168.1.112:8096
ENV JELLYFIN_ACCESS_TOKEN=""

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# Use custom entrypoint to handle environment variable substitution
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]