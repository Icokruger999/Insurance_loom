// Services Dropdown Functionality

// API Base URL - Automatically detects environment
const API_BASE_URL = (() => {
    const hostname = window.location.hostname;
    
    // Production (AWS)
    if (hostname === 'insuranceloom.com' || hostname === 'www.insuranceloom.com') {
        return 'https://api.insuranceloom.com/api';
    }
    
    // Development (localhost)
    return 'http://localhost:5000/api';
})();

// Load services for dropdown
async function loadServicesForDropdown() {
    try {
        const response = await fetch(`${API_BASE_URL}/servicetypes`);
        if (response.ok) {
            const services = await response.json();
            const servicesList = document.getElementById('servicesList');
            if (servicesList && services.length > 0) {
                servicesList.innerHTML = services.map(service => `
                    <li class="services-dropdown-item">
                        <a href="#" class="services-dropdown-link" data-service-id="${service.id}" data-service-name="${service.serviceName}">
                            <strong>${service.serviceName}</strong>
                            <small>${service.description || 'Apply for this insurance product'}</small>
                        </a>
                    </li>
                `).join('');
                
                // Add click handlers
                servicesList.querySelectorAll('.services-dropdown-link').forEach(link => {
                    link.addEventListener('click', (e) => {
                        e.preventDefault();
                        const serviceId = link.dataset.serviceId;
                        const serviceName = link.dataset.serviceName;
                        handleServiceSelection(serviceId, serviceName);
                    });
                });
            } else if (servicesList) {
                servicesList.innerHTML = `
                    <li class="services-dropdown-item">
                        <p style="padding: 1rem; color: var(--text-muted); text-align: center;">No services available</p>
                    </li>
                `;
            }
        }
    } catch (error) {
        console.error('Failed to load services:', error);
        const servicesList = document.getElementById('servicesList');
        if (servicesList) {
            servicesList.innerHTML = `
                <li class="services-dropdown-item">
                    <p style="padding: 1rem; color: var(--danger-color); text-align: center;">Error loading services</p>
                </li>
            `;
        }
    }
}

// Initialize services dropdown
function initServicesDropdown() {
    const dropdownBtn = document.getElementById('servicesDropdownBtn');
    const dropdown = document.getElementById('servicesDropdown');
    
    if (!dropdownBtn || !dropdown) return;
    
    // Toggle dropdown on button click
    dropdownBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        dropdown.classList.toggle('show');
    });
    
    // Close dropdown when clicking outside
    document.addEventListener('click', (e) => {
        if (!dropdown.contains(e.target) && !dropdownBtn.contains(e.target)) {
            dropdown.classList.remove('show');
        }
    });
    
    // Close dropdown on ESC key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && dropdown.classList.contains('show')) {
            dropdown.classList.remove('show');
        }
    });
}

// Handle service selection
function handleServiceSelection(serviceId, serviceName) {
    // Close dropdown
    const dropdown = document.getElementById('servicesDropdown');
    if (dropdown) {
        dropdown.classList.remove('show');
    }
    
    // Redirect to application page with service information
    const applicationUrl = `/application.html?serviceId=${encodeURIComponent(serviceId)}&serviceName=${encodeURIComponent(serviceName)}`;
    window.location.href = applicationUrl;
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    loadServicesForDropdown();
    initServicesDropdown();
});

