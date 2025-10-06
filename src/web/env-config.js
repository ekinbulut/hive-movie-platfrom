// Environment configuration for development
// This file is used when running outside Docker
if (typeof window !== 'undefined' && !window.ENV) {
    window.ENV = {
        AUTH_BASE_URL: 'http://localhost:8082',
        MOVIES_BASE_URL: 'http://localhost:8080'
    };
}