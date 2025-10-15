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
            if (data.expirationTime && Date.now() < data.expirationTime) {
                return data;
            }
            // Token expired, remove it
            localStorage.removeItem('authToken');
        }
        
        return null;
    }
    
    static clearTokens() {
        sessionStorage.removeItem('authToken');
        localStorage.removeItem('authToken');
    }
}

// Auth form switching
class AuthFormSwitcher {
    constructor() {
        this.currentForm = 'login';
        this.init();
    }

    init() {
        // Get toggle buttons and form containers
        this.toggleButtons = document.querySelectorAll('.toggle-btn');
        this.loginContainer = document.getElementById('loginFormContainer');
        this.registerContainer = document.getElementById('registerFormContainer');

        // Add event listeners to toggle buttons
        this.toggleButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                const targetForm = e.target.dataset.form;
                this.switchToForm(targetForm);
            });
        });
    }

    switchToForm(formType) {
        if (formType === this.currentForm) return;

        // Update toggle buttons
        this.toggleButtons.forEach(btn => {
            btn.classList.remove('active');
            if (btn.dataset.form === formType) {
                btn.classList.add('active');
            }
        });

        // Switch forms with animation
        if (formType === 'login') {
            this.registerContainer.classList.remove('active');
            setTimeout(() => {
                this.loginContainer.classList.add('active');
            }, 150);
        } else {
            this.loginContainer.classList.remove('active');
            setTimeout(() => {
                this.registerContainer.classList.add('active');
            }, 150);
        }

        this.currentForm = formType;
        
        // Clear any error messages
        this.hideAllMessages();
    }

    hideAllMessages() {
        const messages = document.querySelectorAll('.error-message, .success-message');
        messages.forEach(msg => msg.style.display = 'none');
    }
}

// Login functionality
class LoginHandler {
    constructor() {
        this.form = document.getElementById('loginForm');
        this.button = document.getElementById('loginButton');
        this.buttonText = this.button.querySelector('.button-text');
        this.loadingSpinner = this.button.querySelector('.loading-spinner');
        this.errorMessage = document.getElementById('loginErrorMessage');
        
        this.init();
    }

    init() {
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
        
        // Clear error messages when user starts typing
        ['loginUsername', 'loginPassword'].forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.addEventListener('input', () => this.hideError());
            }
        });
    }

    async handleSubmit(e) {
        e.preventDefault();
        
        this.hideError();
        
        const username = document.getElementById('loginUsername').value.trim();
        const password = document.getElementById('loginPassword').value;

        if (!username || !password) {
            this.showError('Please fill in all fields');
            return;
        }

        try {
            this.setLoadingState(true);

            console.log('Attempting login with username:', username);

            const response = await fetch(API_CONFIG.AUTH_LOGIN_ENDPOINT, {
                method: 'POST',
                mode: 'cors',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username, password })
            });

            console.log('Login response status:', response.status);
            console.log('Response headers:', Object.fromEntries(response.headers.entries()));

            if (!response.ok) {
                let errorMessage = 'Login failed';
                
                try {
                    const errorData = await response.json();
                    console.log('Error response data:', errorData);
                    errorMessage = errorData.message || errorData.error || errorMessage;
                } catch (e) {
                    console.log('Could not parse error response as JSON:', e);
                    errorMessage = response.statusText || errorMessage;
                }
                
                throw new Error(`${errorMessage} (Status: ${response.status})`);
            }

            const responseData = await response.json();
            console.log('Login successful response:', { ...responseData, accessToken: '[REDACTED]' });

            // Store token using SessionManager
            SessionManager.setAccessToken(
                responseData.accessToken,
                responseData.tokenType || 'Bearer',
                responseData.expiresIn || 3600
            );

            // Redirect to dashboard
            window.location.href = 'dashboard.html';

        } catch (error) {
            console.error('Login error:', error);
            
            let errorMsg = error.message;
            
            if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
                errorMsg = `Cannot connect to API server. Please ensure:\n1. Your API server is running on ${API_CONFIG.AUTH_BASE_URL.replace('http://', '')}\n2. CORS is properly configured on your server\n3. The server accepts requests from this domain`;
            } else if (error.name === 'TypeError' && error.message.includes('NetworkError')) {
                errorMsg = 'Network error - please check your internet connection and server status';
            }
            
            this.showError(errorMsg);
        } finally {
            this.setLoadingState(false);
        }
    }

    setLoadingState(isLoading) {
        this.button.disabled = isLoading;
        this.buttonText.style.display = isLoading ? 'none' : 'inline';
        this.loadingSpinner.style.display = isLoading ? 'inline-flex' : 'none';
    }

    showError(message) {
        this.errorMessage.textContent = message;
        this.errorMessage.style.display = 'block';
        
        setTimeout(() => {
            this.hideError();
        }, 5000);
    }

    hideError() {
        this.errorMessage.style.display = 'none';
    }
}

// Registration functionality
class RegisterHandler {
    constructor() {
        this.form = document.getElementById('registerForm');
        this.button = document.getElementById('registerButton');
        this.buttonText = this.button.querySelector('.button-text');
        this.loadingSpinner = this.button.querySelector('.loading-spinner');
        this.errorMessage = document.getElementById('registerErrorMessage');
        this.successMessage = document.getElementById('registerSuccessMessage');
        
        this.init();
    }

    init() {
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));
        
        // Real-time password confirmation validation
        const passwordInput = document.getElementById('registerPassword');
        const confirmPasswordInput = document.getElementById('registerConfirmPassword');

        const validatePasswordMatch = () => {
            const password = passwordInput.value;
            const confirmPassword = confirmPasswordInput.value;
            
            // Clear all password-related classes first
            confirmPasswordInput.classList.remove('password-mismatch', 'password-match');
            
            if (confirmPassword && password !== confirmPassword) {
                confirmPasswordInput.setCustomValidity('Passwords do not match');
                confirmPasswordInput.classList.add('password-mismatch');
                this.showError('Passwords do not match');
            } else if (confirmPassword && password && password === confirmPassword) {
                confirmPasswordInput.setCustomValidity('');
                confirmPasswordInput.classList.add('password-match');
                this.hideMessages();
            } else {
                confirmPasswordInput.setCustomValidity('');
                this.hideMessages();
            }
        };

        if (passwordInput && confirmPasswordInput) {
            passwordInput.addEventListener('input', validatePasswordMatch);
            confirmPasswordInput.addEventListener('input', validatePasswordMatch);
        }
        
        // Clear messages when user starts typing
        ['registerUsername', 'registerEmail', 'registerPassword', 'registerConfirmPassword'].forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.addEventListener('input', () => this.hideMessages());
            }
        });
    }

    async handleSubmit(e) {
        e.preventDefault();
        
        this.hideMessages();
        
        const formData = {
            username: document.getElementById('registerUsername').value.trim(),
            email: document.getElementById('registerEmail').value.trim(),
            password: document.getElementById('registerPassword').value,
            confirmPassword: document.getElementById('registerConfirmPassword').value
        };

        try {
            this.validateForm(formData);
            this.setLoadingState(true);

            // Prepare data for API (exclude confirmPassword)
            const { confirmPassword, ...apiData } = formData;

            console.log('Attempting registration with:', { 
                username: apiData.username, 
                email: apiData.email,
                endpoint: API_CONFIG.AUTH_REGISTER_ENDPOINT
            });

            const response = await fetch(API_CONFIG.AUTH_REGISTER_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(apiData)
            });

            const responseData = await response.text();
            console.log('Registration response status:', response.status);
            console.log('Registration response:', responseData);

            if (!response.ok) {
                let errorMsg = 'Registration failed';
                
                try {
                    const errorData = JSON.parse(responseData);
                    errorMsg = errorData.message || errorData.error || errorMsg;
                } catch (parseError) {
                    errorMsg = responseData || errorMsg;
                }
                
                throw new Error(errorMsg);
            }

            // Registration successful
            console.log('Registration successful');
            this.showSuccess('Account created successfully! You can now sign in.');
            
            // Switch to login form after 2 seconds
            setTimeout(() => {
                window.authFormSwitcher.switchToForm('login');
                this.form.reset();
            }, 2000);

        } catch (error) {
            console.error('Registration error:', error);
            
            let errorMsg = error.message;
            
            if (error.name === 'TypeError' && error.message.includes('fetch')) {
                errorMsg = 'Unable to connect to server. Please check your connection.';
            } else if (errorMsg.includes('400')) {
                errorMsg = 'Invalid registration data. Please check your inputs.';
            } else if (errorMsg.includes('409')) {
                errorMsg = 'Username or email already exists. Please choose different ones.';
            } else if (errorMsg.includes('500')) {
                errorMsg = 'Server error. Please try again later.';
            }
            
            this.showError(errorMsg);
        } finally {
            this.setLoadingState(false);
        }
    }

    validateForm(formData) {
        const { username, email, password, confirmPassword } = formData;

        if (!username || username.length < 3) {
            throw new Error('Username must be at least 3 characters long');
        }

        if (!email || !this.isValidEmail(email)) {
            throw new Error('Please enter a valid email address');
        }

        if (!password || password.length < 6) {
            throw new Error('Password must be at least 6 characters long');
        }

        if (password !== confirmPassword) {
            throw new Error('Passwords do not match');
        }
    }

    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    setLoadingState(isLoading) {
        this.button.disabled = isLoading;
        this.buttonText.style.display = isLoading ? 'none' : 'inline';
        this.loadingSpinner.style.display = isLoading ? 'inline-flex' : 'none';
    }

    showError(message) {
        this.errorMessage.textContent = message;
        this.errorMessage.style.display = 'block';
        this.successMessage.style.display = 'none';
        
        setTimeout(() => {
            this.hideMessages();
        }, 5000);
    }

    showSuccess(message) {
        this.successMessage.textContent = message;
        this.successMessage.style.display = 'block';
        this.errorMessage.style.display = 'none';
    }

    hideMessages() {
        this.errorMessage.style.display = 'none';
        this.successMessage.style.display = 'none';
        
        // Clear visual password validation states from form inputs
        const confirmPasswordInput = document.getElementById('registerConfirmPassword');
        if (confirmPasswordInput) {
            confirmPasswordInput.classList.remove('password-mismatch', 'password-match');
        }
    }
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Check if user is already logged in
    const token = SessionManager.getAccessToken();
    if (token) {
        console.log('User already logged in, redirecting to dashboard');
        window.location.href = 'dashboard.html';
        return;
    }

    // Initialize form switcher and handlers
    window.authFormSwitcher = new AuthFormSwitcher();
    new LoginHandler();
    new RegisterHandler();
});