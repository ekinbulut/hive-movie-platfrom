// API Configuration - Environment aware
const API_CONFIG = {
    AUTH_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:5188' 
        : 'http://hive-idm:8082',
    get AUTH_LOGIN_ENDPOINT() {
        return `${this.AUTH_BASE_URL}/auth/login`;
    },
    get AUTH_REGISTER_ENDPOINT() {
        return `${this.AUTH_BASE_URL}/auth/register`;
    }
};

// Override with environment variables if available (Docker)
if (typeof window !== 'undefined' && window.ENV) {
    if (window.ENV.AUTH_BASE_URL) {
        API_CONFIG.AUTH_BASE_URL = window.ENV.AUTH_BASE_URL;
    }
}

// Session management utilities
class SessionManager {
    static setAccessToken(accessToken, tokenType, expiresIn) {
        const tokenData = {
            accessToken,
            tokenType,
            expiresIn,
            timestamp: Date.now()
        };
        
        // Store in sessionStorage (expires when tab is closed)
        sessionStorage.setItem('authToken', JSON.stringify(tokenData));
        
        // Also store in localStorage with expiration for persistence
        const expirationTime = Date.now() + (expiresIn * 1000);
        const persistentData = {
            ...tokenData,
            expirationTime
        };
        localStorage.setItem('authToken', JSON.stringify(persistentData));
    }
    
    static getAccessToken() {
        // First try sessionStorage
        let tokenData = sessionStorage.getItem('authToken');
        if (tokenData) {
            return JSON.parse(tokenData);
        }
        
        // Then try localStorage with expiration check
        tokenData = localStorage.getItem('authToken');
        if (tokenData) {
            const data = JSON.parse(tokenData);
            if (Date.now() < data.expirationTime) {
                // Token is still valid, restore to sessionStorage
                sessionStorage.setItem('authToken', JSON.stringify(data));
                return data;
            } else {
                // Token expired, remove it
                localStorage.removeItem('authToken');
            }
        }
        
        return null;
    }
    
    static clearAccessToken() {
        sessionStorage.removeItem('authToken');
        localStorage.removeItem('authToken');
    }
    
    static isAuthenticated() {
        const tokenData = this.getAccessToken();
        if (!tokenData) return false;
        
        // Check if token is expired
        const expirationTime = tokenData.timestamp + (tokenData.expiresIn * 1000);
        return Date.now() < expirationTime;
    }
}

// API utilities
class AuthAPI {
    static async login(username, password) {
        console.log('Attempting login to:', API_CONFIG.AUTH_LOGIN_ENDPOINT);
        console.log('Request payload:', { username, password: '[REDACTED]' });
        
        try {
            const response = await fetch(API_CONFIG.AUTH_LOGIN_ENDPOINT, {
                method: 'POST',
                mode: 'cors',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    username,
                    password
                })
            });
            
            console.log('Response status:', response.status);
            console.log('Response headers:', Object.fromEntries(response.headers.entries()));
            
            if (!response.ok) {
                let errorMessage = 'Login failed';
                
                try {
                    const errorData = await response.json();
                    console.log('Error response data:', errorData);
                    errorMessage = errorData.message || errorData.error || errorMessage;
                } catch (e) {
                    console.log('Could not parse error response as JSON:', e);
                    // If response is not JSON, use status text
                    errorMessage = response.statusText || errorMessage;
                }
                
                throw new Error(`${errorMessage} (Status: ${response.status})`);
            }
            
            const responseData = await response.json();
            console.log('Successful response:', { ...responseData, accessToken: '[REDACTED]' });
            return responseData;
            
        } catch (error) {
            console.error('Fetch error:', error);
            
            if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
                throw new Error(`Cannot connect to API server. Please ensure:\n1. Your API server is running on ${API_CONFIG.AUTH_BASE_URL.replace('http://', '')}\n2. CORS is properly configured on your server\n3. The server accepts requests from localhost:8000`);
            }
            
            if (error.name === 'TypeError' && error.message.includes('NetworkError')) {
                throw new Error('Network error - please check your internet connection and server status');
            }
            
            throw error;
        }
    }
}

// DOM elements
const loginForm = document.getElementById('loginForm');
const usernameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');
const loginButton = document.getElementById('loginButton');
const buttonText = loginButton.querySelector('.button-text');
const loadingSpinner = loginButton.querySelector('.loading-spinner');
const errorMessage = document.getElementById('errorMessage');

// UI utility functions
function showLoading() {
    loginButton.disabled = true;
    buttonText.style.display = 'none';
    loadingSpinner.style.display = 'inline-block';
}

function hideLoading() {
    loginButton.disabled = false;
    buttonText.style.display = 'inline-block';
    loadingSpinner.style.display = 'none';
}

function showError(message) {
    errorMessage.textContent = message;
    errorMessage.style.display = 'block';
    
    // Auto-hide error after 5 seconds
    setTimeout(() => {
        hideError();
    }, 5000);
}

function hideError() {
    errorMessage.style.display = 'none';
}

// Form validation
function validateForm() {
    const username = usernameInput.value.trim();
    const password = passwordInput.value.trim();
    
    if (!username) {
        showError('Please enter your username');
        usernameInput.focus();
        return false;
    }
    
    if (!password) {
        showError('Please enter your password');
        passwordInput.focus();
        return false;
    }
    
    return true;
}

// Event handlers
async function handleLogin(event) {
    event.preventDefault();
    
    // Hide any previous errors
    hideError();
    
    // Validate form
    if (!validateForm()) {
        return;
    }
    
    const username = usernameInput.value.trim();
    const password = passwordInput.value.trim();
    
    try {
        showLoading();
        
        // Make API call
        const response = await AuthAPI.login(username, password);
        
        // Store token in session
        SessionManager.setAccessToken(
            response.accessToken,
            response.tokenType,
            response.expiresIn
        );
        
        console.log('Login successful:', {
            tokenType: response.tokenType,
            expiresIn: response.expiresIn,
            tokenStored: true
        });
        
        // Redirect directly to dashboard
        console.log('User authenticated, redirecting to dashboard...');
        window.location.href = 'dashboard.html';
        
    } catch (error) {
        console.error('Login error:', error);
        showError(error.message || 'Login failed. Please try again.');
    } finally {
        hideLoading();
    }
}

// Event listeners
loginForm.addEventListener('submit', handleLogin);

// Clear error when user starts typing
usernameInput.addEventListener('input', hideError);
passwordInput.addEventListener('input', hideError);

// Test API server connectivity
async function testAPIConnectivity() {
    try {
        console.log('Testing API server connectivity...');
        const response = await fetch(API_CONFIG.AUTH_LOGIN_ENDPOINT, {
            method: 'OPTIONS'
        });
        console.log('API server connectivity test - Status:', response.status);
        return true;
    } catch (error) {
        console.error('API server connectivity test failed:', error);
        showError(`⚠️ Cannot connect to API server at ${API_CONFIG.AUTH_BASE_URL.replace('http://', '')}. Please ensure your backend server is running.`);
        return false;
    }
}

// Check if user is already authenticated on page load
document.addEventListener('DOMContentLoaded', async () => {
    console.log('Page loaded from:', window.location.href);
    
    if (SessionManager.isAuthenticated()) {
        const tokenData = SessionManager.getAccessToken();
        console.log('User already authenticated:', tokenData);
        
        // Optionally redirect to main application
        // window.location.href = '/dashboard';
        
        // Or show a message
        showError('You are already logged in!');
    }
    
    // Test API connectivity
    await testAPIConnectivity();
    
    // Focus username input for better UX
    usernameInput.focus();
});

// Export for potential use in other modules
window.SessionManager = SessionManager;
window.AuthAPI = AuthAPI;