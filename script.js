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

function handleBrokerLogin(e) {
    e.preventDefault();
    // TODO: Connect to C# backend API endpoint for broker login
    // Example: window.location.href = '/broker/login';
    // Or: fetch('/api/auth/broker/login', { method: 'POST', ... })
    console.log('Broker login button clicked - ready for C# backend integration');
    // For now, just show a placeholder message
    alert('Broker login functionality will be connected to C# backend');
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

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
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

