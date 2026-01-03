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

// Login Button Handlers (ready for C# backend integration)
const brokerLoginBtn = document.getElementById('brokerLoginBtn');
const brokerLoginBtnMobile = document.getElementById('brokerLoginBtnMobile');
const policyHolderLoginBtn = document.getElementById('policyHolderLoginBtn');
const policyHolderLoginBtnMobile = document.getElementById('policyHolderLoginBtnMobile');

// API Base URL - Automatically detects environment
const API_BASE_URL = (() => {
    const hostname = window.location.hostname;
    
    // Production (AWS)
    if (hostname === 'insuranceloom.com' || hostname === 'www.insuranceloom.com') {
        // Use EC2 IP address (update to api.insuranceloom.com once DNS is configured)
        return 'http://34.246.222.13/api';
    }
    
    // Development (localhost)
    return 'http://localhost:5000/api';
})();

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

function handleBrokerLogin(e) {
    e.preventDefault();
    e.stopPropagation();
    console.log('Broker login button clicked');
    openBrokerModal();
}

function handlePolicyHolderLogin(e) {
    e.preventDefault();
    // TODO: Connect to C# backend API endpoint for policy holder login
    // Example: window.location.href = '/policyholder/login';
    // Or: fetch('/api/auth/policyholder/login', { method: 'POST', ... })
    console.log('Policy holder login button clicked - ready for C# backend integration');
    // For now, just show a placeholder message
    alert('Policy holder login functionality will be connected to C# backend');
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
        if (e.target === brokerModal) {
            closeBrokerModal();
        }
    });
}

// Broker Login Form Handler
if (brokerLoginForm) {
    brokerLoginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorDiv = document.getElementById('loginError');
        errorDiv.classList.remove('show');

        const formData = {
            agentNumber: document.getElementById('loginAgentNumber').value,
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
                alert(`Login successful! Welcome ${data.broker?.firstName || 'Broker'}`);
                closeBrokerModal();
                // You can redirect or update UI here
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
            companyName: document.getElementById('regCompanyName').value || null,
            phone: document.getElementById('regPhone').value || null,
            licenseNumber: document.getElementById('regLicenseNumber').value || null
            // CommissionRate uses default value (5%), can be changed later
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

if (brokerLoginBtn) {
    brokerLoginBtn.addEventListener('click', handleBrokerLogin);
}

if (brokerLoginBtnMobile) {
    brokerLoginBtnMobile.addEventListener('click', handleBrokerLogin);
}

if (policyHolderLoginBtn) {
    policyHolderLoginBtn.addEventListener('click', handlePolicyHolderLogin);
}

if (policyHolderLoginBtnMobile) {
    policyHolderLoginBtnMobile.addEventListener('click', handlePolicyHolderLogin);
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

