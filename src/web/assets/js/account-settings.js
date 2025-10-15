// Account Settings JavaScript

// API Configuration - Environment aware
const API_CONFIG = {
    AUTH_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:5188' 
        : 'http://hive-idm:0882',
    MOVIES_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:5154' 
        : 'http://hive-app:8080',
    get USER_INFO_ENDPOINT() {
        return `${this.AUTH_BASE_URL}/user/info`;
    },
    get APP_SETTINGS_ENDPOINT() {
        return `${this.MOVIES_BASE_URL}/v1/api/settings`;
    }
};

// Override with environment variables if available (Docker)
if (typeof window !== 'undefined' && window.ENV) {
    if (window.ENV.AUTH_BASE_URL) {
        API_CONFIG.AUTH_BASE_URL = window.ENV.AUTH_BASE_URL;
    }
    if (window.ENV.MOVIES_BASE_URL) {
        API_CONFIG.MOVIES_BASE_URL = window.ENV.MOVIES_BASE_URL;
    }
}

// Session management utilities (same as other pages)
class SessionManager {
    static setAccessToken(accessToken, tokenType, expiresIn) {
        const tokenData = {
            accessToken,
            tokenType,
            expiresIn,
            timestamp: Date.now()
        };
        
        sessionStorage.setItem('authToken', JSON.stringify(tokenData));
        
        const expirationTime = Date.now() + (expiresIn * 1000);
        const persistentData = {
            ...tokenData,
            expirationTime
        };
        localStorage.setItem('authToken', JSON.stringify(persistentData));
    }
    
    static getAccessToken() {
        let tokenData = sessionStorage.getItem('authToken');
        if (tokenData) {
            return JSON.parse(tokenData);
        }
        
        tokenData = localStorage.getItem('authToken');
        if (tokenData) {
            const data = JSON.parse(tokenData);
            if (data.expirationTime && Date.now() < data.expirationTime) {
                sessionStorage.setItem('authToken', JSON.stringify(data));
                return data;
            } else {
                this.clearAccessToken();
            }
        }
        return null;
    }
    
    static clearAccessToken() {
        sessionStorage.removeItem('authToken');
        localStorage.removeItem('authToken');
    }
    
    static isAuthenticated() {
        return this.getAccessToken() !== null;
    }
}

// Account Settings Manager
class AccountSettingsManager {
    constructor() {
        this.currentUserData = null;
        this.originalData = null;
        this.currentAppSettings = null;
        this.originalAppSettings = null;
        
        this.initializeElements();
        this.attachEventListeners();
        this.checkAuthentication();
    }

    initializeElements() {
        // Navigation elements
        this.accountBtn = document.getElementById('accountBtn');
        this.dropdownMenu = document.getElementById('dropdownMenu');
        this.logoutBtn = document.getElementById('logoutBtn');
        this.settingsBtn = document.getElementById('settingsBtn');
        this.backBtn = document.getElementById('backBtn');
        
        // Personal Info Form elements
        this.accountForm = document.getElementById('accountForm');
        this.firstNameInput = document.getElementById('firstName');
        this.lastNameInput = document.getElementById('lastName');
        this.emailInput = document.getElementById('email');
        this.saveBtn = document.getElementById('saveBtn');
        this.cancelBtn = document.getElementById('cancelBtn');
        this.btnText = document.querySelector('#saveBtn .btn-text');
        this.loadingSpinner = document.querySelector('#saveBtn .loading-spinner');
        
        // App Settings Form elements
        this.appSettingsForm = document.getElementById('appSettingsForm');
        this.mediaFolderPathInput = document.getElementById('mediaFolderPath');
        this.appSaveBtn = document.getElementById('appSaveBtn');
        this.appCancelBtn = document.getElementById('appCancelBtn');
        this.appBtnText = document.querySelector('#appSaveBtn .btn-text');
        this.appLoadingSpinner = document.querySelector('#appSaveBtn .loading-spinner');
        
        // Message elements
        this.messageContainer = document.getElementById('messageContainer');
        this.messageText = document.getElementById('messageText');
        this.appMessageContainer = document.getElementById('appMessageContainer');
        this.appMessageText = document.getElementById('appMessageText');
    }

    attachEventListeners() {
        // Navigation events
        if (this.accountBtn && this.dropdownMenu) {
            this.accountBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.toggleAccountDropdown();
            });
        }
        
        if (this.logoutBtn) {
            this.logoutBtn.addEventListener('click', () => this.logout());
        }
        
        if (this.settingsBtn) {
            this.settingsBtn.addEventListener('click', () => {
                // Already on settings page
                this.closeAccountDropdown();
            });
        }
        
        if (this.backBtn) {
            this.backBtn.addEventListener('click', () => this.goBackToDashboard());
        }
        
        // Personal Info Form events
        if (this.accountForm) {
            this.accountForm.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }
        
        if (this.cancelBtn) {
            this.cancelBtn.addEventListener('click', () => this.resetForm());
        }
        
        // App Settings Form events
        if (this.appSettingsForm) {
            this.appSettingsForm.addEventListener('submit', (e) => this.handleAppSettingsSubmit(e));
        }
        
        if (this.appCancelBtn) {
            this.appCancelBtn.addEventListener('click', () => this.resetAppSettingsForm());
        }
        
        // Input change detection
        [this.firstNameInput, this.lastNameInput].forEach(input => {
            if (input) {
                input.addEventListener('input', () => this.checkForChanges());
            }
        });
        
        if (this.mediaFolderPathInput) {
            this.mediaFolderPathInput.addEventListener('input', () => this.checkForAppSettingsChanges());
        }
        
        // Close dropdown when clicking outside
        document.addEventListener('click', () => {
            this.closeAccountDropdown();
        });
        
        // Prevent dropdown from closing when clicking inside it
        if (this.dropdownMenu) {
            this.dropdownMenu.addEventListener('click', (e) => {
                e.stopPropagation();
            });
        }
    }

    async checkAuthentication() {
        if (!SessionManager.isAuthenticated()) {
            window.location.href = 'login.html';
            return;
        }
        
        await Promise.all([
            this.loadUserData(),
            this.loadAppSettings()
        ]);
    }

    async loadUserData() {
        try {
            const tokenData = SessionManager.getAccessToken();
            if (!tokenData) {
                throw new Error('No access token found');
            }

            const response = await fetch(API_CONFIG.USER_INFO_ENDPOINT, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${tokenData.accessToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    SessionManager.clearAccessToken();
                    window.location.href = 'login.html';
                    return;
                }
                throw new Error(`Failed to load user data: ${response.status}`);
            }

            const userData = await response.json();
            // Map the response to our internal structure
            this.currentUserData = {
                firstName: userData.name || '',
                lastName: userData.surname || '',
                email: userData.email || ''
            };
            this.originalData = { ...this.currentUserData };
            this.populateForm();
            
        } catch (error) {
            console.error('Error loading user data:', error);
            this.showMessage('Failed to load user information. Please try again.', 'error');
        }
    }

    populateForm() {
        if (this.currentUserData) {
            if (this.firstNameInput) {
                this.firstNameInput.value = this.currentUserData.firstName || '';
            }
            if (this.lastNameInput) {
                this.lastNameInput.value = this.currentUserData.lastName || '';
            }
            if (this.emailInput) {
                this.emailInput.value = this.currentUserData.email || '';
            }
        }
        this.checkForChanges();
    }

    async loadAppSettings() {
        try {
            const tokenData = SessionManager.getAccessToken();
            if (!tokenData) {
                throw new Error('No access token found');
            }

            const response = await fetch(API_CONFIG.APP_SETTINGS_ENDPOINT, {
                method: 'GET',
                headers: {
                    'Authorization': `${tokenData.tokenType} ${tokenData.accessToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    SessionManager.clearAccessToken();
                    window.location.href = 'login.html';
                    return;
                }
                // If settings don't exist, use defaults
                if (response.status === 404) {
                    this.currentAppSettings = { mediaFolderPath: '' };
                    this.originalAppSettings = { ...this.currentAppSettings };
                    this.populateAppSettingsForm();
                    return;
                }
                throw new Error(`Failed to load app settings: ${response.status}`);
            }

            this.currentAppSettings = await response.json();
            this.originalAppSettings = { ...this.currentAppSettings };
            this.populateAppSettingsForm();
            
        } catch (error) {
            console.error('Error loading app settings:', error);
            // Use defaults if loading fails
            this.currentAppSettings = { mediaFolderPath: '' };
            this.originalAppSettings = { ...this.currentAppSettings };
            this.populateAppSettingsForm();
        }
    }

    populateAppSettingsForm() {
        if (this.currentAppSettings) {
            if (this.mediaFolderPathInput) {
                this.mediaFolderPathInput.value = this.currentAppSettings.mediaFolderPath || '';
            }
        }
        this.checkForAppSettingsChanges();
    }

    checkForChanges() {
        if (!this.originalData || !this.saveBtn) return;
        
        const hasChanges = 
            this.firstNameInput.value.trim() !== (this.originalData.firstName || '') ||
            this.lastNameInput.value.trim() !== (this.originalData.lastName || '');
        
        this.saveBtn.disabled = !hasChanges;
    }

    checkForAppSettingsChanges() {
        if (!this.originalAppSettings || !this.appSaveBtn) return;
        
        const hasChanges = 
            this.mediaFolderPathInput.value.trim() !== (this.originalAppSettings.mediaFolderPath || '');
        
        this.appSaveBtn.disabled = !hasChanges;
    }

    async handleFormSubmit(e) {
        e.preventDefault();
        
        if (this.saveBtn.disabled) return;
        
        this.setLoading(true);
        this.hideMessage();
        
        try {
            const tokenData = SessionManager.getAccessToken();
            if (!tokenData) {
                throw new Error('No access token found');
            }

            // Map form data to API structure
            const updateData = {
                firstName: this.firstNameInput.value.trim(),
                lastName: this.lastNameInput.value.trim()
            };

            // Assume PUT endpoint follows same pattern as GET
            const response = await fetch(API_CONFIG.USER_INFO_ENDPOINT, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${tokenData.accessToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                if (response.status === 401) {
                    SessionManager.clearAccessToken();
                    window.location.href = 'login.html';
                    return;
                }
                
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || `Failed to update profile: ${response.status}`);
            }

            // Reload user data to get the updated information
            await this.loadUserData();
            
            this.showMessage('Profile updated successfully!', 'success');
            this.checkForChanges();
            
        } catch (error) {
            console.error('Error updating profile:', error);
            this.showMessage(error.message || 'Failed to update profile. Please try again.', 'error');
        } finally {
            this.setLoading(false);
        }
    }

    resetForm() {
        this.populateForm();
        this.hideMessage();
    }

    async handleAppSettingsSubmit(e) {
        e.preventDefault();
        
        if (this.appSaveBtn.disabled) return;
        
        this.setAppLoading(true);
        this.hideAppMessage();
        
        try {
            const tokenData = SessionManager.getAccessToken();
            if (!tokenData) {
                throw new Error('No access token found');
            }

            const updateData = {
                mediaFolderPath: this.mediaFolderPathInput.value.trim()
            };

            const response = await fetch(API_CONFIG.APP_SETTINGS_ENDPOINT, {
                method: 'PUT',
                headers: {
                    'Authorization': `${tokenData.tokenType} ${tokenData.accessToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                if (response.status === 401) {
                    SessionManager.clearAccessToken();
                    window.location.href = 'login.html';
                    return;
                }
                
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || `Failed to update app settings: ${response.status}`);
            }

            const updatedSettings = await response.json();
            this.currentAppSettings = updatedSettings;
            this.originalAppSettings = { ...updatedSettings };
            
            this.showAppMessage('App settings updated successfully!', 'success');
            this.checkForAppSettingsChanges();
            
        } catch (error) {
            console.error('Error updating app settings:', error);
            this.showAppMessage(error.message || 'Failed to update app settings. Please try again.', 'error');
        } finally {
            this.setAppLoading(false);
        }
    }

    resetAppSettingsForm() {
        this.populateAppSettingsForm();
        this.hideAppMessage();
    }

    setLoading(isLoading) {
        if (!this.saveBtn || !this.btnText || !this.loadingSpinner) return;
        
        this.saveBtn.disabled = isLoading;
        
        if (isLoading) {
            this.btnText.style.display = 'none';
            this.loadingSpinner.style.display = 'inline-block';
        } else {
            this.btnText.style.display = 'inline';
            this.loadingSpinner.style.display = 'none';
        }
    }

    setAppLoading(isLoading) {
        if (!this.appSaveBtn || !this.appBtnText || !this.appLoadingSpinner) return;
        
        this.appSaveBtn.disabled = isLoading;
        
        if (isLoading) {
            this.appBtnText.style.display = 'none';
            this.appLoadingSpinner.style.display = 'inline-block';
        } else {
            this.appBtnText.style.display = 'inline';
            this.appLoadingSpinner.style.display = 'none';
        }
    }

    showMessage(text, type = 'success') {
        if (!this.messageContainer || !this.messageText) return;
        
        this.messageText.textContent = text;
        this.messageText.className = `message ${type}`;
        this.messageContainer.style.display = 'block';
        
        // Auto-hide success messages after 5 seconds
        if (type === 'success') {
            setTimeout(() => this.hideMessage(), 5000);
        }
    }

    hideMessage() {
        if (this.messageContainer) {
            this.messageContainer.style.display = 'none';
        }
    }

    showAppMessage(text, type = 'success') {
        if (!this.appMessageContainer || !this.appMessageText) return;
        
        this.appMessageText.textContent = text;
        this.appMessageText.className = `message ${type}`;
        this.appMessageContainer.style.display = 'block';
        
        // Auto-hide success messages after 5 seconds
        if (type === 'success') {
            setTimeout(() => this.hideAppMessage(), 5000);
        }
    }

    hideAppMessage() {
        if (this.appMessageContainer) {
            this.appMessageContainer.style.display = 'none';
        }
    }

    // Navigation methods
    toggleAccountDropdown() {
        if (!this.dropdownMenu) return;
        
        const isVisible = this.dropdownMenu.classList.contains('show');
        if (isVisible) {
            this.closeAccountDropdown();
        } else {
            this.openAccountDropdown();
        }
    }
    
    openAccountDropdown() {
        if (this.dropdownMenu) {
            this.dropdownMenu.classList.add('show');
        }
    }
    
    closeAccountDropdown() {
        if (this.dropdownMenu) {
            this.dropdownMenu.classList.remove('show');
        }
    }

    goBackToDashboard() {
        window.location.href = 'dashboard.html';
    }

    logout() {
        this.closeAccountDropdown();
        if (confirm('Are you sure you want to logout?')) {
            SessionManager.clearAccessToken();
            window.location.href = 'login.html';
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new AccountSettingsManager();
});