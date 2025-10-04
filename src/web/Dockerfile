# Use official nginx image as base
FROM nginx:alpine

# Set maintainer label
LABEL maintainer="project-hive"
LABEL description="Project Hive Web Application - Movie Collection Dashboard"

# Remove default nginx website
RUN rm -rf /usr/share/nginx/html/*

# Copy the web application files to nginx html directory
COPY . /usr/share/nginx/html/

# Create nginx configuration for Single Page Application
RUN cat > /etc/nginx/conf.d/default.conf <<EOF
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # Enable gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;

    # Handle favicon
    location = /favicon.ico {
        log_not_found off;
        access_log off;
        return 204;
    }

    # Handle static assets with caching
    location ~* \.(css|js|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        access_log off;
    }

    # Handle specific HTML files in pages directory
    location ~ ^/pages/.+\.html$ {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }

    # Handle root and SPA routing
    location / {
        # First attempt to serve request as file, then as directory, then fall back to index.html
        try_files \$uri \$uri/index.html /index.html;
        
        # Add cache control for HTML files
        location ~ \.html$ {
            add_header Cache-Control "no-cache, no-store, must-revalidate";
            add_header Pragma "no-cache";
            add_header Expires "0";
        }
    }
}
EOF

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