// API Configuration (use relative base to work on any port/host)
const API_BASE_URL = '/api';

// API Service Class
class ApiService {
    constructor() {
        this.baseURL = API_BASE_URL;
    }

    // Generic request method
    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const token = localStorage.getItem('accessToken');
        
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` })
            }
        };

        const config = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, config);
            
            // Handle token refresh for 401 errors
            if (response.status === 401 && token) {
                const refreshed = await this.refreshToken();
                if (refreshed) {
                    // Retry the original request with new token
                    const newToken = localStorage.getItem('accessToken');
                    config.headers.Authorization = `Bearer ${newToken}`;
                    const retryResponse = await fetch(url, config);
                    return this.handleResponse(retryResponse);
                } else {
                    // Refresh failed, redirect to login
                    this.logout();
                    throw new Error('Authentication failed');
                }
            }

            return this.handleResponse(response);
        } catch (error) {
            console.error('API Request failed:', error);
            throw error;
        }
    }

    // Handle response and extract JSON
    async handleResponse(response) {
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
        }
        // No Content
        if (response.status === 204 || response.status === 205) {
            return null;
        }
        // Some endpoints may return empty body with 2xx
        const contentLength = response.headers.get('content-length');
        if (contentLength === '0' || contentLength === null) {
            const text = await response.text();
            if (!text) return null;
            try { return JSON.parse(text); } catch { return null; }
        }
        return response.json();
    }

    // Refresh token
    async refreshToken() {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
            return false;
        }

        try {
            const response = await fetch(`${this.baseURL}/auth/refresh`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ refreshToken })
            });

            if (response.ok) {
                const data = await response.json();
                localStorage.setItem('accessToken', data.accessToken);
                return true;
            }
        } catch (error) {
            console.error('Token refresh failed:', error);
        }

        return false;
    }

    // Logout and clear tokens
    logout() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.reload();
    }

    // Authentication API
    async login(credentials) {
        return this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify(credentials)
        });
    }

    async loginOrCreate(credentials) {
        return this.request('/auth/login-or-create', {
            method: 'POST',
            body: JSON.stringify(credentials)
        });
    }

    // Users API
    async getUsers() {
        return this.request('/users');
    }

    async getUser(id) {
        return this.request(`/users/${id}`);
    }

    async createUser(userData) {
        return this.request('/users', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    }

    async updateUser(id, userData) {
        return this.request(`/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify(userData)
        });
    }

    async deleteUser(id) {
        return this.request(`/users/${id}`, {
            method: 'DELETE'
        });
    }

    // Roles API
    async getRoles() {
        return this.request('/roles');
    }

    async getRole(id) {
        return this.request(`/roles/${id}`);
    }

    async createRole(roleData) {
        return this.request('/roles', {
            method: 'POST',
            body: JSON.stringify(roleData)
        });
    }

    async updateRole(id, roleData) {
        return this.request(`/roles/${id}`, {
            method: 'PUT',
            body: JSON.stringify(roleData)
        });
    }

    async deleteRole(id) {
        return this.request(`/roles/${id}`, {
            method: 'DELETE'
        });
    }
}

// Create global API instance
const api = new ApiService();

// Utility functions
const showError = (message, elementId = null) => {
    const errorElement = elementId ? document.getElementById(elementId) : document.querySelector('.error-message');
    if (errorElement) {
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    } else {
        console.error('Error:', message);
    }
};

const hideError = (elementId = null) => {
    const errorElement = elementId ? document.getElementById(elementId) : document.querySelector('.error-message');
    if (errorElement) {
        errorElement.style.display = 'none';
    }
};

const showLoading = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.display = 'block';
    }
};

const hideLoading = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.display = 'none';
    }
};

const showLoadingOverlay = () => {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.style.display = 'flex';
    }
};

const hideLoadingOverlay = () => {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
};

// Export for use in other scripts
window.api = api;
window.showError = showError;
window.hideError = hideError;
window.showLoading = showLoading;
window.hideLoading = hideLoading;
window.showLoadingOverlay = showLoadingOverlay;
window.hideLoadingOverlay = hideLoadingOverlay;
