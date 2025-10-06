#!/bin/sh

# Replace environment variables in the template file
envsubst '${AUTH_BASE_URL} ${MOVIES_BASE_URL}' < /usr/share/nginx/html/env-config.js.template > /usr/share/nginx/html/env-config.js

# Remove the template file
rm /usr/share/nginx/html/env-config.js.template

# Start nginx
exec nginx -g "daemon off;"