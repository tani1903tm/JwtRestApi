// Application State
let currentUser = null;
let users = [];
let roles = [];
let currentTab = 'users';

// DOM Elements
const loginPage = document.getElementById('loginPage');
const dashboardPage = document.getElementById('dashboardPage');
const loginForm = document.getElementById('loginForm');
const logoutBtn = document.getElementById('logoutBtn');
const usersTab = document.getElementById('usersTab');
const rolesTab = document.getElementById('rolesTab');
const usersSection = document.getElementById('usersSection');
const rolesSection = document.getElementById('rolesSection');
const usersList = document.getElementById('usersList');
const rolesList = document.getElementById('rolesList');
const welcomeMessage = document.getElementById('welcomeMessage');

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
    setupEventListeners();
});

function initializeApp() {
    // Check if user is already logged in
    const token = localStorage.getItem('accessToken');
    if (token) {
        showDashboard();
        loadData();
    } else {
        showLogin();
    }
}

function setupEventListeners() {
    // Login form
    loginForm.addEventListener('submit', handleLogin);
    
    // Logout button
    logoutBtn.addEventListener('click', handleLogout);
    
    // Tab navigation
    usersTab.addEventListener('click', () => switchTab('users'));
    rolesTab.addEventListener('click', () => switchTab('roles'));
    
    // User management
    document.getElementById('addUserBtn').addEventListener('click', () => openUserModal());
    document.getElementById('closeUserModal').addEventListener('click', closeUserModal);
    document.getElementById('cancelUserBtn').addEventListener('click', closeUserModal);
    document.getElementById('userForm').addEventListener('submit', handleUserSubmit);
    
    // Role management
    document.getElementById('addRoleBtn').addEventListener('click', () => openRoleModal());
    document.getElementById('closeRoleModal').addEventListener('click', closeRoleModal);
    document.getElementById('cancelRoleBtn').addEventListener('click', closeRoleModal);
    document.getElementById('roleForm').addEventListener('submit', handleRoleSubmit);
    
    // Modal close on outside click
    window.addEventListener('click', function(event) {
        const userModal = document.getElementById('userModal');
        const roleModal = document.getElementById('roleModal');
        
        if (event.target === userModal) {
            closeUserModal();
        }
        if (event.target === roleModal) {
            closeRoleModal();
        }
    });
}

// Authentication functions
async function handleLogin(event) {
    event.preventDefault();
    
    const formData = new FormData(loginForm);
    const autoCreate = formData.get('autoCreateUser') === 'on';
    
    const credentials = {
        usernameOrEmail: formData.get('usernameOrEmail'),
        password: formData.get('password'),
        autoCreate: autoCreate
    };
    
    const loginBtn = document.getElementById('loginBtn');
    const loginBtnText = document.getElementById('loginBtnText');
    const loginSpinner = document.getElementById('loginSpinner');
    
    try {
        // Show loading state
        loginBtn.disabled = true;
        loginBtnText.style.display = 'none';
        loginSpinner.style.display = 'inline-block';
        hideError('loginError');
        
        const response = autoCreate ? 
            await api.loginOrCreate(credentials) : 
            await api.login(credentials);
        
        // Store tokens
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        
        // Set current user (you might want to get this from the API response)
        currentUser = {
            username: credentials.usernameOrEmail,
            email: credentials.usernameOrEmail
        };
        
        showDashboard();
        loadData();
        
    } catch (error) {
        showError(error.message, 'loginError');
    } finally {
        // Reset loading state
        loginBtn.disabled = false;
        loginBtnText.style.display = 'inline';
        loginSpinner.style.display = 'none';
    }
}

function handleLogout() {
    api.logout();
}

// Page navigation
function showLogin() {
    loginPage.style.display = 'block';
    dashboardPage.style.display = 'none';
}

function showDashboard() {
    loginPage.style.display = 'none';
    dashboardPage.style.display = 'block';
    welcomeMessage.textContent = `Welcome, ${currentUser?.username || 'User'}!`;
}

// Tab management
function switchTab(tab) {
    currentTab = tab;
    
    // Update tab buttons
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.getElementById(tab + 'Tab').classList.add('active');
    
    // Update content sections
    document.querySelectorAll('.content-section').forEach(s => s.style.display = 'none');
    document.getElementById(tab + 'Section').style.display = 'block';
    
    // Load data for the active tab
    if (tab === 'users') {
        loadUsers();
    } else if (tab === 'roles') {
        loadRoles();
    }
}

// Data loading functions
async function loadData() {
    try {
        showLoadingOverlay();
        await Promise.all([loadUsers(), loadRoles()]);
    } catch (error) {
        console.error('Failed to load data:', error);
        showError('Failed to load data. Please refresh the page.');
    } finally {
        hideLoadingOverlay();
    }
}

async function loadUsers() {
    try {
        showLoading('usersLoading');
        users = await api.getUsers();
        renderUsers();
        updateUsersCount();
    } catch (error) {
        console.error('Failed to load users:', error);
        usersList.innerHTML = '<div class="error-message">Failed to load users. Please try again.</div>';
    } finally {
        hideLoading('usersLoading');
    }
}

async function loadRoles() {
    try {
        showLoading('rolesLoading');
        roles = await api.getRoles();
        renderRoles();
        updateRolesCount();
    } catch (error) {
        console.error('Failed to load roles:', error);
        rolesList.innerHTML = '<div class="error-message">Failed to load roles. Please try again.</div>';
    } finally {
        hideLoading('rolesLoading');
    }
}

// Rendering functions
function renderUsers() {
    if (users.length === 0) {
        usersList.innerHTML = `
            <div class="empty-state">
                <h3>No users found</h3>
                <p>Get started by adding your first user.</p>
                <button class="btn btn-primary" onclick="openUserModal()">Add User</button>
            </div>
        `;
        return;
    }
    
    usersList.innerHTML = users.map(user => `
        <div class="list-item">
            <div class="list-item-content">
                <div class="avatar">${user.username.charAt(0).toUpperCase()}</div>
                <div class="item-info">
                    <h3>${user.username}</h3>
                    <p>${user.email}</p>
                </div>
            </div>
            <div class="item-actions">
                <button class="btn btn-sm btn-secondary" onclick="editUser(${user.id})">Edit</button>
                <button class="btn btn-sm btn-danger" onclick="deleteUser(${user.id})">Delete</button>
            </div>
        </div>
    `).join('');
}

function renderRoles() {
    if (roles.length === 0) {
        rolesList.innerHTML = `
            <div class="empty-state">
                <h3>No roles found</h3>
                <p>Get started by adding your first role.</p>
                <button class="btn btn-primary" onclick="openRoleModal()">Add Role</button>
            </div>
        `;
        return;
    }
    
    rolesList.innerHTML = roles.map(role => `
        <div class="list-item">
            <div class="list-item-content">
                <div class="item-info">
                    <h3>${role.name}</h3>
                    <p>${role.description || 'No description'}</p>
                </div>
            </div>
            <div class="item-actions">
                <button class="btn btn-sm btn-secondary" onclick="editRole(${role.id})">Edit</button>
                <button class="btn btn-sm btn-danger" onclick="deleteRole(${role.id})">Delete</button>
            </div>
        </div>
    `).join('');
}

function updateUsersCount() {
    const countElement = document.getElementById('usersCount');
    if (countElement) {
        countElement.textContent = `(${users.length})`;
    }
}

function updateRolesCount() {
    const countElement = document.getElementById('rolesCount');
    if (countElement) {
        countElement.textContent = `(${roles.length})`;
    }
}

// User management functions
function openUserModal(userId = null) {
    const modal = document.getElementById('userModal');
    const form = document.getElementById('userForm');
    const title = document.getElementById('userModalTitle');
    
    if (userId) {
        const user = users.find(u => u.id === userId);
        if (user) {
            title.textContent = 'Edit User';
            document.getElementById('userId').value = user.id;
            document.getElementById('userUsername').value = user.username;
            document.getElementById('userEmail').value = user.email;
            document.getElementById('userPassword').value = '';
            document.getElementById('userPassword').required = false;
        }
    } else {
        title.textContent = 'Add User';
        form.reset();
        document.getElementById('userPassword').required = true;
    }
    
    loadRolesForSelect();
    modal.style.display = 'flex';
}

function closeUserModal() {
    document.getElementById('userModal').style.display = 'none';
}

async function loadRolesForSelect() {
    const select = document.getElementById('userRoles');
    select.innerHTML = roles.map(role => 
        `<option value="${role.id}">${role.name}</option>`
    ).join('');
}

async function handleUserSubmit(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const userData = {
        username: formData.get('username'),
        email: formData.get('email'),
        password: formData.get('password'),
        roles: Array.from(formData.getAll('roles')).map(Number)
    };
    
    const userId = formData.get('id');
    
    try {
        showLoadingOverlay();
        
        if (userId) {
            await api.updateUser(userId, userData);
        } else {
            await api.createUser(userData);
        }
        
        closeUserModal();
        await loadUsers();
        
    } catch (error) {
        console.error('Failed to save user:', error);
        showError('Failed to save user. Please try again.');
    } finally {
        hideLoadingOverlay();
    }
}

async function editUser(userId) {
    openUserModal(userId);
}

async function deleteUser(userId) {
    if (confirm('Are you sure you want to delete this user?')) {
        try {
            showLoadingOverlay();
            await api.deleteUser(userId);
            await loadUsers();
        } catch (error) {
            console.error('Failed to delete user:', error);
            showError('Failed to delete user. Please try again.');
        } finally {
            hideLoadingOverlay();
        }
    }
}

// Role management functions
function openRoleModal(roleId = null) {
    const modal = document.getElementById('roleModal');
    const form = document.getElementById('roleForm');
    const title = document.getElementById('roleModalTitle');
    
    if (roleId) {
        const role = roles.find(r => r.id === roleId);
        if (role) {
            title.textContent = 'Edit Role';
            document.getElementById('roleId').value = role.id;
            document.getElementById('roleName').value = role.name;
            document.getElementById('roleDescription').value = role.description || '';
        }
    } else {
        title.textContent = 'Add Role';
        form.reset();
    }
    
    modal.style.display = 'flex';
}

function closeRoleModal() {
    document.getElementById('roleModal').style.display = 'none';
}

async function handleRoleSubmit(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const roleData = {
        name: formData.get('name'),
        description: formData.get('description')
    };
    
    const roleId = formData.get('id');
    
    try {
        showLoadingOverlay();
        
        if (roleId) {
            await api.updateRole(roleId, roleData);
        } else {
            await api.createRole(roleData);
        }
        
        closeRoleModal();
        await loadRoles();
        
    } catch (error) {
        console.error('Failed to save role:', error);
        showError('Failed to save role. Please try again.');
    } finally {
        hideLoadingOverlay();
    }
}

async function editRole(roleId) {
    openRoleModal(roleId);
}

async function deleteRole(roleId) {
    if (confirm('Are you sure you want to delete this role?')) {
        try {
            showLoadingOverlay();
            await api.deleteRole(roleId);
            await loadRoles();
        } catch (error) {
            console.error('Failed to delete role:', error);
            showError('Failed to delete role. Please try again.');
        } finally {
            hideLoadingOverlay();
        }
    }
}

// Global functions for HTML onclick handlers
window.editUser = editUser;
window.deleteUser = deleteUser;
window.editRole = editRole;
window.deleteRole = deleteRole;
window.openUserModal = openUserModal;
window.openRoleModal = openRoleModal;
