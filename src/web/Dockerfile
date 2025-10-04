# Use official nginx image as base
FROM nginx:alpine

# Set maintainer label
LABEL maintainer="project-hive"
LABEL description="Project Hive Web Application - Movie Collection Dashboard"

# Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*

# Copy the web application files to nginx html directory (exclude Docker/config files)
COPY src/web/index.html /usr/share/nginx/html/
COPY src/web/assets/ /usr/share/nginx/html/assets/
COPY src/web/pages/ /usr/share/nginx/html/pages/
COPY src/web/tools/ /usr/share/nginx/html/tools/

# Copy nginx configuration
COPY src/web/nginx.conf /etc/nginx/conf.d/default.conf

# Set proper permissions
RUN chmod -R 755 /usr/share/nginx/html
RUN chown -R nginx:nginx /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]