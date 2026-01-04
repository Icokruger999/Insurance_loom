// Mobile Navigation Toggle - Initialize after DOM is ready
let navToggle = null;
let navMenu = null;

function initMobileMenu() {
    const toggle = document.querySelector('.nav-toggle');
    const menu = document.querySelector('.nav-menu');
    
    console.log('Initializing mobile menu:', { toggle, menu });
    
    if (toggle && menu) {
        // Remove any existing event listeners by cloning
        const newToggle = toggle.cloneNode(true);
        toggle.parentNode.replaceChild(newToggle, toggle);
        
        // Add click event to the new toggle
        newToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('Hamburger clicked, current state:', menu.classList.contains('active'));
            menu.classList.toggle('active');
            console.log('After toggle, state:', menu.classList.contains('active'));
        });
        
        // Close mobile menu when clicking on a link
        const navLinks = menu.querySelectorAll('a');
        navLinks.forEach(link => {
            link.addEventListener('click', function() {
                menu.classList.remove('active');
            });
        });
        
        console.log('Mobile menu initialized successfully');
    } else {
        console.error('Mobile menu elements not found:', { toggle, menu });
    }
}

// Login Button Handlers - will be initialized in DOMContentLoaded
let loginBtn = null;
const loginTypeModal = document.getElementById('loginTypeModal');
const selectBrokerBtn = document.getElementById('selectBrokerBtn');
const selectManagerBtn = document.getElementById('selectManagerBtn');
const selectClientBtn = document.getElementById('selectClientBtn');
const closeLoginTypeModal = document.getElementById('closeLoginTypeModal');
const clientModal = document.getElementById('clientModal');
const managerModal = document.getElementById('managerModal');
const managerLoginForm = document.getElementById('managerLoginForm');
const managerForgotPasswordLink = document.getElementById('managerForgotPasswordLink');
const managerForgotPasswordModal = document.getElementById('managerForgotPasswordModal');
const managerForgotPasswordForm = document.getElementById('managerForgotPasswordForm');
const closeManagerForgotPasswordModal = document.getElementById('closeManagerForgotPasswordModal');
const backToManagerLoginLink = document.getElementById('backToManagerLoginLink');
const brokerForgotPasswordLink = document.getElementById('brokerForgotPasswordLink');
const brokerForgotPasswordModal = document.getElementById('brokerForgotPasswordModal');
const brokerForgotPasswordForm = document.getElementById('brokerForgotPasswordForm');
const closeBrokerForgotPasswordModal = document.getElementById('closeBrokerForgotPasswordModal');
const backToBrokerLoginLink = document.getElementById('backToBrokerLoginLink');
const clientForgotPasswordLink = document.getElementById('clientForgotPasswordLink');
const clientForgotPasswordModal = document.getElementById('clientForgotPasswordModal');
const clientForgotPasswordForm = document.getElementById('clientForgotPasswordForm');
const closeClientForgotPasswordModal = document.getElementById('closeClientForgotPasswordModal');
const backToClientLoginLink = document.getElementById('backToClientLoginLink');
const changePasswordModal = document.getElementById('changePasswordModal');
const changePasswordForm = document.getElementById('changePasswordForm');
const closeChangePasswordModal = document.getElementById('closeChangePasswordModal');
let currentUserType = null; // 'manager', 'broker', or 'client'

// API Base URL - Automatically detects environment
// Use window object to avoid duplicate declaration errors
if (typeof window.API_BASE_URL === 'undefined') {
    window.API_BASE_URL = (() => {
        const hostname = window.location.hostname;
        
        // Production (AWS)
        if (hostname === 'insuranceloom.com' || hostname === 'www.insuranceloom.com') {
            // Use HTTPS API subdomain
            return 'https://api.insuranceloom.com/api';
        }
        
        // Development (localhost)
        return 'http://localhost:5000/api';
    })();
}
// Reference to API_BASE_URL for convenience (use window.API_BASE_URL in code to avoid redeclaration)

// Load companies list on page load
async function loadCompanies() {
    try {
        const response = await fetch(`${window.API_BASE_URL}/company?activeOnly=true`);
        if (response.ok) {
            const companies = await response.json();
            const companyList = document.getElementById('companyList');
            if (companyList) {
                companyList.innerHTML = '';
                companies.forEach(company => {
                    const option = document.createElement('option');
                    option.value = company.name;
                    companyList.appendChild(option);
                });
            }
        }
    } catch (error) {
        console.error('Failed to load companies:', error);
    }
}

// Load companies and services when page loads
document.addEventListener('DOMContentLoaded', () => {
    loadCompanies();
    loadServicesForDropdown();
    initServicesDropdown();
    initMobileMenu(); // Initialize mobile menu toggle
    
    // Login button event listener - get element after DOM is ready
    const loginButton = document.getElementById('loginBtn');
    if (loginButton) {
        loginButton.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            handleLogin(e);
        });
        console.log('Login button event listener attached');
    } else {
        console.error('Login button not found');
    }
});

// Broker Modal
const brokerModal = document.getElementById('brokerModal');
const modalClose = document.querySelector('.modal-close');
const tabButtons = document.querySelectorAll('.tab-button');
const tabContents = document.querySelectorAll('.tab-content');
const brokerLoginForm = document.getElementById('brokerLoginForm');
const brokerRegisterForm = document.getElementById('brokerRegisterForm');

function openBrokerModal() {
    // Get modal element dynamically in case DOM isn't ready when script loads
    const modal = document.getElementById('brokerModal');
    if (!modal) {
        console.error('Broker modal not found in DOM');
        return;
    }
    console.log('Opening broker modal');
    modal.classList.add('active');
    // Reset to login tab
    switchTab('login');
}

function closeBrokerModal() {
    const modal = document.getElementById('brokerModal');
    if (modal) {
        modal.classList.remove('active');
    }
    // Reset forms
    if (brokerLoginForm) brokerLoginForm.reset();
    if (brokerRegisterForm) brokerRegisterForm.reset();
    const loginError = document.getElementById('loginError');
    const registerError = document.getElementById('registerError');
    const registerSuccess = document.getElementById('registerSuccess');
    if (loginError) loginError.classList.remove('show');
    if (registerError) registerError.classList.remove('show');
    if (registerSuccess) registerSuccess.classList.remove('show');
}

function switchTab(tabName) {
    // Update tab buttons
    tabButtons.forEach(btn => {
        if (btn.dataset.tab === tabName) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    });

    // Update tab contents
    tabContents.forEach(content => {
        if (content.id === `${tabName}Tab`) {
            content.classList.add('active');
        } else {
            content.classList.remove('active');
        }
    });
}

function openLoginTypeModal() {
    const modal = document.getElementById('loginTypeModal');
    if (!modal) {
        console.error('Login type modal not found in DOM');
        return;
    }
    modal.classList.add('active');
}

function closeLoginTypeModalFunc() {
    const modal = document.getElementById('loginTypeModal');
    if (modal) {
        modal.classList.remove('active');
    }
}

function openClientModal() {
    closeLoginTypeModalFunc();
    const modal = document.getElementById('clientModal');
    if (!modal) {
        console.error('Client modal not found in DOM');
        return;
    }
    modal.classList.add('active');
}

function closeClientModal() {
    const modal = document.getElementById('clientModal');
    if (modal) {
        modal.classList.remove('active');
    }
}

function openManagerModal() {
    const modal = document.getElementById('managerModal');
    if (!modal) {
        console.error('Manager modal not found in DOM');
        return;
    }
    modal.classList.add('active');
}

function closeManagerModal() {
    const modal = document.getElementById('managerModal');
    if (modal) {
        modal.classList.remove('active');
    }
    if (managerLoginForm) managerLoginForm.reset();
    const managerError = document.getElementById('managerLoginError');
    if (managerError) managerError.classList.remove('show');
}

function handleLogin(e) {
    if (e) {
        e.preventDefault();
        e.stopPropagation();
    }
    console.log('Login button clicked');
    openLoginTypeModal();
}

// Tab switching
tabButtons.forEach(btn => {
    btn.addEventListener('click', () => {
        switchTab(btn.dataset.tab);
    });
});

// Close modal events
if (modalClose) {
    modalClose.addEventListener('click', closeBrokerModal);
}

if (brokerModal) {
    brokerModal.addEventListener('click', (e) => {
        // Only close if clicking directly on the modal backdrop (not on modal-content or its children)
        const modalContent = brokerModal.querySelector('.modal-content');
        if (e.target === brokerModal) {
            closeBrokerModal();
        }
    });
    
    // Prevent clicks inside modal-content from bubbling to brokerModal
    const modalContent = brokerModal.querySelector('.modal-content');
    if (modalContent) {
        modalContent.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }
}

// Broker Login Form Handler
if (brokerLoginForm) {
    brokerLoginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('loginError');
        errorDiv.classList.remove('show');

        const formData = {
            email: document.getElementById('loginEmail').value,
            password: document.getElementById('loginPassword').value
        };

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/broker/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (response.ok) {
                // Store token if needed
                if (data.token) {
                    localStorage.setItem('brokerToken', data.token);
                    localStorage.setItem('brokerInfo', JSON.stringify(data.broker));
                }
                closeBrokerModal();
                
                // Check if password looks temporary (12 chars, alphanumeric) and prompt for change
                const password = formData.password;
                if (password.length === 12 && /^[a-zA-Z0-9]+$/.test(password)) {
                    currentUserType = 'broker';
                    if (changePasswordModal) {
                        changePasswordModal.classList.add('active');
                    }
                } else {
                    // Redirect to broker portal
                    window.location.href = '/broker-portal.html';
                }
            } else {
                errorDiv.textContent = data.message || 'Login failed. Please check your credentials.';
                errorDiv.classList.add('show');
            }
        } catch (error) {
            console.error('Login error:', error);
            errorDiv.textContent = 'Connection error. Please make sure the API is running on ' + window.API_BASE_URL;
            errorDiv.classList.add('show');
        }
    });
}

// Broker Registration Form Handler
if (brokerRegisterForm) {
    brokerRegisterForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('registerError');
        const successDiv = document.getElementById('registerSuccess');
        errorDiv.classList.remove('show');
        successDiv.classList.remove('show');

        const formData = {
            email: document.getElementById('regEmail').value,
            password: document.getElementById('regPassword').value,
            // AgentNumber is auto-generated by the backend
            firstName: document.getElementById('regFirstName').value,
            lastName: document.getElementById('regLastName').value,
            companyName: document.getElementById('regCompanyName').value.trim() || null,
            managerEmail: document.getElementById('regManagerEmail').value.trim(),
            phone: document.getElementById('regPhone').value || null,
            licenseNumber: document.getElementById('regLicenseNumber').value || null
            // CommissionRate uses default value, can be changed later
        };

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/broker/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (response.ok) {
                successDiv.textContent = `Registration successful! Agent Number: ${data.agentNumber}. You can now login.`;
                successDiv.classList.add('show');
                brokerRegisterForm.reset();
                // Switch to login tab after 2 seconds
                setTimeout(() => {
                    switchTab('login');
                    successDiv.classList.remove('show');
                }, 2000);
            } else {
                errorDiv.textContent = data.message || 'Registration failed. Please check your information.';
                errorDiv.classList.add('show');
            }
        } catch (error) {
            console.error('Registration error:', error);
            errorDiv.textContent = 'Connection error. Please make sure the API is running on ' + window.API_BASE_URL;
            errorDiv.classList.add('show');
        }
    });
}

// Login button handler is attached in DOMContentLoaded event - no duplicate needed


// Login Type Selection
if (selectBrokerBtn) {
    selectBrokerBtn.addEventListener('click', () => {
        closeLoginTypeModalFunc();
        openBrokerModal();
    });
}

if (selectManagerBtn) {
    selectManagerBtn.addEventListener('click', () => {
        closeLoginTypeModalFunc();
        openManagerModal();
    });
}

if (selectClientBtn) {
    selectClientBtn.addEventListener('click', () => {
        openClientModal();
    });
}

// Close Login Type Modal
if (closeLoginTypeModal) {
    closeLoginTypeModal.addEventListener('click', closeLoginTypeModalFunc);
}

if (loginTypeModal) {
    loginTypeModal.addEventListener('click', (e) => {
        if (e.target === loginTypeModal) {
            closeLoginTypeModalFunc();
        }
    });
    
    // Prevent clicks inside modal-content from closing the modal
    const modalContent = loginTypeModal.querySelector('.modal-content');
    if (modalContent) {
        modalContent.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }
}

// Close Client Modal
const clientModalClose = clientModal ? clientModal.querySelector('.modal-close') : null;
if (clientModalClose) {
    clientModalClose.addEventListener('click', closeClientModal);
}

if (clientModal) {
    clientModal.addEventListener('click', (e) => {
        if (e.target === clientModal) {
            closeClientModal();
        }
    });
}

// Manager Modal
const closeManagerModalBtn = managerModal ? managerModal.querySelector('.modal-close') : null;
if (closeManagerModalBtn) {
    closeManagerModalBtn.addEventListener('click', closeManagerModal);
}

if (managerModal) {
    managerModal.addEventListener('click', (e) => {
        if (e.target === managerModal) {
            closeManagerModal();
        }
    });
    // Prevent clicks inside modal-content from closing the modal
    const modalContent = managerModal.querySelector('.modal-content');
    if (modalContent) {
        modalContent.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }
}

// Manager Forgot Password Modal Handlers
if (managerForgotPasswordLink) {
    managerForgotPasswordLink.addEventListener('click', (e) => {
        e.preventDefault();
        closeManagerModal();
        if (managerForgotPasswordModal) {
            managerForgotPasswordModal.classList.add('active');
        }
    });
}

if (closeManagerForgotPasswordModal) {
    closeManagerForgotPasswordModal.addEventListener('click', () => {
        if (managerForgotPasswordModal) {
            managerForgotPasswordModal.classList.remove('active');
        }
    });
}

if (backToManagerLoginLink) {
    backToManagerLoginLink.addEventListener('click', (e) => {
        e.preventDefault();
        if (managerForgotPasswordModal) {
            managerForgotPasswordModal.classList.remove('active');
        }
        if (managerModal) {
            managerModal.classList.add('active');
        }
    });
}

// Manager Forgot Password Form Handler
if (managerForgotPasswordForm) {
    managerForgotPasswordForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('managerForgotPasswordError');
        const successDiv = document.getElementById('managerForgotPasswordSuccess');
        if (errorDiv) errorDiv.classList.remove('show');
        if (successDiv) successDiv.style.display = 'none';

        const email = document.getElementById('managerForgotPasswordEmail').value;

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/manager/forgot-password`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email })
            });

            const data = await response.json();

            if (response.ok) {
                if (successDiv) {
                    successDiv.textContent = 'Password reset email has been sent. Please check your inbox for your temporary password.';
                    successDiv.style.display = 'block';
                }
                if (managerForgotPasswordForm) managerForgotPasswordForm.reset();
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Failed to send reset link. Please try again.';
                    errorDiv.classList.add('show');
                }
            }
        } catch (error) {
            console.error('Forgot password error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'Connection error. Please make sure the API is running.';
                errorDiv.classList.add('show');
            }
        }
    });
}

// Broker Forgot Password Handlers
if (brokerForgotPasswordLink) {
    brokerForgotPasswordLink.addEventListener('click', (e) => {
        e.preventDefault();
        closeBrokerModal();
        if (brokerForgotPasswordModal) {
            brokerForgotPasswordModal.classList.add('active');
        }
    });
}

if (closeBrokerForgotPasswordModal) {
    closeBrokerForgotPasswordModal.addEventListener('click', () => {
        if (brokerForgotPasswordModal) {
            brokerForgotPasswordModal.classList.remove('active');
        }
    });
}

if (backToBrokerLoginLink) {
    backToBrokerLoginLink.addEventListener('click', (e) => {
        e.preventDefault();
        if (brokerForgotPasswordModal) {
            brokerForgotPasswordModal.classList.remove('active');
        }
        if (brokerModal) {
            brokerModal.classList.add('active');
        }
    });
}

if (brokerForgotPasswordForm) {
    brokerForgotPasswordForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('brokerForgotPasswordError');
        const successDiv = document.getElementById('brokerForgotPasswordSuccess');
        if (errorDiv) errorDiv.classList.remove('show');
        if (successDiv) successDiv.style.display = 'none';

        const email = document.getElementById('brokerForgotPasswordEmail').value;

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/broker/forgot-password`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email })
            });

            const data = await response.json();

            if (response.ok) {
                if (successDiv) {
                    successDiv.textContent = 'Password reset email has been sent. Please check your inbox for your temporary password.';
                    successDiv.style.display = 'block';
                }
                if (brokerForgotPasswordForm) brokerForgotPasswordForm.reset();
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Failed to send reset link. Please try again.';
                    errorDiv.classList.add('show');
                }
            }
        } catch (error) {
            console.error('Broker forgot password error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'Connection error. Please make sure the API is running.';
                errorDiv.classList.add('show');
            }
        }
    });
}

// Client Forgot Password Handlers
if (clientForgotPasswordLink) {
    clientForgotPasswordLink.addEventListener('click', (e) => {
        e.preventDefault();
        closeClientModal();
        if (clientForgotPasswordModal) {
            clientForgotPasswordModal.classList.add('active');
        }
    });
}

if (closeClientForgotPasswordModal) {
    closeClientForgotPasswordModal.addEventListener('click', () => {
        if (clientForgotPasswordModal) {
            clientForgotPasswordModal.classList.remove('active');
        }
    });
}

if (backToClientLoginLink) {
    backToClientLoginLink.addEventListener('click', (e) => {
        e.preventDefault();
        if (clientForgotPasswordModal) {
            clientForgotPasswordModal.classList.remove('active');
        }
        if (clientModal) {
            clientModal.classList.add('active');
        }
    });
}

if (clientForgotPasswordForm) {
    clientForgotPasswordForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('clientForgotPasswordError');
        const successDiv = document.getElementById('clientForgotPasswordSuccess');
        if (errorDiv) errorDiv.classList.remove('show');
        if (successDiv) successDiv.style.display = 'none';

        const email = document.getElementById('clientForgotPasswordEmail').value;

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/client/forgot-password`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email })
            });

            const data = await response.json();

            if (response.ok) {
                if (successDiv) {
                    successDiv.textContent = 'Password reset email has been sent. Please check your inbox for your temporary password.';
                    successDiv.style.display = 'block';
                }
                if (clientForgotPasswordForm) clientForgotPasswordForm.reset();
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Failed to send reset link. Please try again.';
                    errorDiv.classList.add('show');
                }
            }
        } catch (error) {
            console.error('Client forgot password error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'Connection error. Please make sure the API is running.';
                errorDiv.classList.add('show');
            }
        }
    });
}

// Change Password Modal Handler
if (closeChangePasswordModal) {
    closeChangePasswordModal.addEventListener('click', () => {
        if (changePasswordModal) {
            changePasswordModal.classList.remove('active');
        }
    });
}

if (changePasswordModal) {
    changePasswordModal.addEventListener('click', (e) => {
        if (e.target === changePasswordModal) {
            changePasswordModal.classList.remove('active');
        }
    });
    
    const modalContent = changePasswordModal.querySelector('.modal-content');
    if (modalContent) {
        modalContent.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }
}

if (changePasswordForm) {
    changePasswordForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('changePasswordError');
        const successDiv = document.getElementById('changePasswordSuccess');
        if (errorDiv) errorDiv.classList.remove('show');
        if (successDiv) successDiv.style.display = 'none';

        const newPassword = document.getElementById('newPassword').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (newPassword !== confirmPassword) {
            if (errorDiv) {
                errorDiv.textContent = 'New passwords do not match.';
                errorDiv.classList.add('show');
            }
            return;
        }

        if (newPassword.length < 6) {
            if (errorDiv) {
                errorDiv.textContent = 'Password must be at least 6 characters long.';
                errorDiv.classList.add('show');
            }
            return;
        }

        const currentPassword = document.getElementById('currentPassword').value;
        const token = localStorage.getItem('managerToken') || localStorage.getItem('brokerToken') || localStorage.getItem('clientToken');
        
        if (!token) {
            if (errorDiv) {
                errorDiv.textContent = 'You must be logged in to change your password.';
                errorDiv.classList.add('show');
            }
            return;
        }

        // Determine user type from token storage
        let endpoint = '';
        if (localStorage.getItem('managerToken')) {
            endpoint = '/auth/manager/change-password';
            currentUserType = 'manager';
        } else if (localStorage.getItem('brokerToken')) {
            endpoint = '/auth/broker/change-password';
            currentUserType = 'broker';
        } else if (localStorage.getItem('clientToken')) {
            endpoint = '/auth/client/change-password';
            currentUserType = 'client';
        }

        try {
            const response = await fetch(`${window.API_BASE_URL}${endpoint}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    currentPassword: currentPassword,
                    newPassword: newPassword
                })
            });

            const data = await response.json();

            if (response.ok) {
                if (successDiv) {
                    successDiv.textContent = 'Password changed successfully! You can now use your new password to log in.';
                    successDiv.style.display = 'block';
                }
                if (changePasswordForm) changePasswordForm.reset();
                
                // Close modal after 2 seconds and redirect to login
                setTimeout(() => {
                    if (changePasswordModal) {
                        changePasswordModal.classList.remove('active');
                    }
                    // Clear tokens and redirect to login
                    localStorage.removeItem('managerToken');
                    localStorage.removeItem('brokerToken');
                    localStorage.removeItem('clientToken');
                    localStorage.removeItem('managerInfo');
                    localStorage.removeItem('brokerInfo');
                    window.location.href = '/';
                }, 2000);
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Failed to change password. Please try again.';
                    errorDiv.classList.add('show');
                }
            }
        } catch (error) {
            console.error('Change password error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'Connection error. Please make sure the API is running.';
                errorDiv.classList.add('show');
            }
        }
    });
}

// Function to check if password is temporary and prompt for change
function checkAndPromptPasswordChange(userType, token) {
    // For now, we'll prompt after login if they used a temporary password
    // This is a simple check - in production, you might want to add a flag in the user table
    // For now, we'll show the change password modal after successful login
    // You can enhance this by checking if the password looks like a temporary one (12 chars, alphanumeric)
}

// Manager Login Form Handler
if (managerLoginForm) {
    managerLoginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('managerLoginError');
        errorDiv.classList.remove('show');

        const formData = {
            email: document.getElementById('managerEmail').value,
            password: document.getElementById('managerPassword').value
        };

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/manager/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (response.ok) {
                // Store token if needed
                if (data.token) {
                    localStorage.setItem('managerToken', data.token);
                    localStorage.setItem('managerInfo', JSON.stringify(data.manager));
                }
                closeManagerModal();
                
                // Check if password looks temporary (12 chars, alphanumeric) and prompt for change
                const password = formData.password;
                if (password.length === 12 && /^[a-zA-Z0-9]+$/.test(password)) {
                    currentUserType = 'manager';
                    if (changePasswordModal) {
                        changePasswordModal.classList.add('active');
                    }
                } else {
                    // Redirect to manager portal
                    window.location.href = '/manager-portal.html';
                }
            } else {
                errorDiv.textContent = data.message || 'Login failed. Please check your credentials.';
                errorDiv.classList.add('show');
            }
        } catch (error) {
            console.error('Manager login error:', error);
            errorDiv.textContent = 'Connection error. Please make sure the API is running on ' + window.API_BASE_URL;
            errorDiv.classList.add('show');
        }
    });
}

// Client Login Form Handler
const clientLoginForm = document.getElementById('clientLoginForm');
if (clientLoginForm) {
    clientLoginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('clientLoginError');
        if (errorDiv) errorDiv.classList.remove('show');

        const formData = {
            email: document.getElementById('clientEmail').value,
            password: document.getElementById('clientPassword').value
        };

        try {
            const response = await fetch(`${window.API_BASE_URL}/auth/client/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (response.ok) {
                // Store token if needed
                if (data.token) {
                    localStorage.setItem('clientToken', data.token);
                    localStorage.setItem('clientInfo', JSON.stringify(data.user));
                }
                closeClientModal();
                
                // Check if password looks temporary (12 chars, alphanumeric) and prompt for change
                const password = formData.password;
                if (password.length === 12 && /^[a-zA-Z0-9]+$/.test(password)) {
                    currentUserType = 'client';
                    if (changePasswordModal) {
                        changePasswordModal.classList.add('active');
                    }
                } else {
                    // Redirect to client dashboard or homepage
                    window.location.href = '/';
                }
            } else {
                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Login failed. Please check your credentials.';
                    errorDiv.classList.add('show');
                }
            }
        } catch (error) {
            console.error('Client login error:', error);
            if (errorDiv) {
                errorDiv.textContent = 'Connection error. Please make sure the API is running.';
                errorDiv.classList.add('show');
            }
        }
    });
}

// Smooth scroll for anchor links (exclude login buttons)
document.querySelectorAll('a[href^="#"]:not(.btn-login)').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Add scroll effect to navbar
let lastScroll = 0;
const navbar = document.querySelector('.navbar');

window.addEventListener('scroll', () => {
    const currentScroll = window.pageYOffset;
    
    if (currentScroll <= 0) {
        navbar.style.boxShadow = 'none';
    } else {
        navbar.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.3)';
    }
    
    lastScroll = currentScroll;
});

// Fade in animation on scroll
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
        }
    });
}, observerOptions);

// Observe feature cards and other sections
document.addEventListener('DOMContentLoaded', () => {
    const featureCards = document.querySelectorAll('.feature-card');
    featureCards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });
});



