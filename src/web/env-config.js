// Environment configuration for development
// This file is used when running outside Docker
if (typeof window !== 'undefined' && !window.ENV) {
    window.ENV = {
        AUTH_BASE_URL: 'http://localhost:5188',
        MOVIES_BASE_URL: 'http://localhost:5154',
        JELLYFIN_BASE_URL: 'http://192.168.1.112:8096',
        JELLYFIN_ACCESS_TOKEN: ''
    };
}