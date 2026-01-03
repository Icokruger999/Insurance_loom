// Mobile Navigation Toggle
const navToggle = document.querySelector('.nav-toggle');
const navMenu = document.querySelector('.nav-menu');

if (navToggle) {
    navToggle.addEventListener('click', () => {
        navMenu.classList.toggle('active');
    });
}

// Close mobile menu when clicking on a link
const navLinks = document.querySelectorAll('.nav-menu a');
navLinks.forEach(link => {
    link.addEventListener('click', () => {
        navMenu.classList.remove('active');
    });
});

// Login Button Handlers
const loginBtn = document.getElementById('loginBtn');
const loginBtnMobile = document.getElementById('loginBtnMobile');
const loginTypeModal = document.getElementById('loginTypeModal');
const selectBrokerBtn = document.getElementById('selectBrokerBtn');
const selectManagerBtn = document.getElementById('selectManagerBtn');
const selectClientBtn = document.getElementById('selectClientBtn');
const closeLoginTypeModal = document.getElementById('closeLoginTypeModal');
const clientModal = document.getElementById('clientModal');
const managerModal = document.getElementById('managerModal');
const managerLoginForm = document.getElementById('managerLoginForm');

// API Base URL - Automatically detects environment
const API_BASE_URL = (() => {
    const hostname = window.location.hostname;
    
    // Production (AWS)
    if (hostname === 'insuranceloom.com' || hostname === 'www.insuranceloom.com') {
        // Use HTTPS API subdomain
        return 'https://api.insuranceloom.com/api';
    }
    
    // Development (localhost)
    return 'http://localhost:5000/api';
})();

// Load companies list on page load
async function loadCompanies() {
    try {
        const response = await fetch(`${API_BASE_URL}/company?activeOnly=true`);
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

// Load companies when page loads
document.addEventListener('DOMContentLoaded', loadCompanies);

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
    e.preventDefault();
    e.stopPropagation();
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
            const response = await fetch(`${API_BASE_URL}/auth/broker/login`, {
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
                // Redirect to broker portal
                window.location.href = '/broker-portal.html';
            } else {
                errorDiv.textContent = data.message || 'Login failed. Please check your credentials.';
                errorDiv.classList.add('show');
            }
        } catch (error) {
            console.error('Login error:', error);
            errorDiv.textContent = 'Connection error. Please make sure the API is running on ' + API_BASE_URL;
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
            const response = await fetch(`${API_BASE_URL}/auth/broker/register`, {
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
            errorDiv.textContent = 'Connection error. Please make sure the API is running on ' + API_BASE_URL;
            errorDiv.classList.add('show');
        }
    });
}

// Single Login Button Handlers
if (loginBtn) {
    loginBtn.addEventListener('click', handleLogin);
}

if (loginBtnMobile) {
    loginBtnMobile.addEventListener('click', handleLogin);
}

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
            const response = await fetch(`${API_BASE_URL}/auth/manager/login`, {
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
                // Redirect to manager portal
                window.location.href = '/manager-portal.html';
            } else {
                errorDiv.textContent = data.message || 'Login failed. Please check your credentials.';
                errorDiv.classList.add('show');
            }
        } catch (error) {
            console.error('Manager login error:', error);
            errorDiv.textContent = 'Connection error. Please make sure the API is running on ' + API_BASE_URL;
            errorDiv.classList.add('show');
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

