// Broker Portal JavaScript

// API Base URL
const API_BASE_URL = (() => {
    const hostname = window.location.hostname;
    
    // Production (AWS)
    if (hostname === 'insuranceloom.com' || hostname === 'www.insuranceloom.com') {
        return 'https://api.insuranceloom.com/api';
    }
    
    // Development (localhost)
    return 'http://localhost:5000/api';
})();

// Check if user is logged in
function checkAuth() {
    const token = localStorage.getItem('brokerToken');
    const brokerInfo = localStorage.getItem('brokerInfo');
    
    if (!token || !brokerInfo) {
        // Not logged in, redirect to home page
        window.location.href = '/';
        return false;
    }
    
    return true;
}

// Load broker information
function loadBrokerInfo() {
    const brokerInfo = localStorage.getItem('brokerInfo');
    if (brokerInfo) {
        try {
            const broker = JSON.parse(brokerInfo);
            const brokerNameEl = document.getElementById('brokerName');
            const brokerEmailEl = document.getElementById('brokerEmail');
            
            if (brokerNameEl) {
                brokerNameEl.textContent = `${broker.firstName || ''} ${broker.lastName || ''}`.trim() || 'Broker';
            }
            
            if (brokerEmailEl) {
                brokerEmailEl.textContent = broker.email || '';
            }
        } catch (error) {
            console.error('Error parsing broker info:', error);
        }
    }
}

// Navigation handling
function initNavigation() {
    const navButtons = document.querySelectorAll('.nav-btn');
    const sections = document.querySelectorAll('.content-section');
    const sectionTitle = document.getElementById('sectionTitle');
    
    const sectionTitles = {
        'create-client': 'Create New Client',
        'new-clients': 'Policies Pending Approval',
        'existing-clients': 'My Policies',
        'monthly-report': 'Monthly Report',
        'commission': 'Commission'
    };
    
    navButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const targetSection = btn.dataset.section;
            
            // Update active nav button
            navButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            
            // Update active section
            sections.forEach(s => s.classList.remove('active'));
            const targetSectionEl = document.getElementById(targetSection);
            if (targetSectionEl) {
                targetSectionEl.classList.add('active');
            }
            
            // Update section title
            if (sectionTitle && sectionTitles[targetSection]) {
                sectionTitle.textContent = sectionTitles[targetSection];
            }
            
            // Load section data
            loadSectionData(targetSection);
        });
    });
}

// Logout functionality
function initLogout() {
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('brokerToken');
            localStorage.removeItem('brokerInfo');
            window.location.href = '/';
        });
    }
}

// Load section-specific data
async function loadSectionData(sectionId) {
    const token = localStorage.getItem('brokerToken');
    const brokerInfo = localStorage.getItem('brokerInfo');
    
    if (!token || !brokerInfo) return;
    
    try {
        const broker = JSON.parse(brokerInfo);
        
        switch (sectionId) {
            case 'create-client':
                loadCreateClientForm();
                break;
            case 'new-clients':
                await loadPendingApprovalPolicies(broker.id);
                break;
            case 'existing-clients':
                await loadAllPolicies(broker.id);
                break;
            case 'monthly-report':
                await loadMonthlyReport(broker.id);
                break;
            case 'commission':
                await loadCommission(broker.id);
                break;
        }
    } catch (error) {
        console.error(`Error loading ${sectionId}:`, error);
    }
}

// Load policies pending approval
async function loadPendingApprovalPolicies(brokerId) {
    const policiesList = document.getElementById('pendingPoliciesList');
    if (!policiesList) return;
    
    policiesList.innerHTML = '<p class="loading-text">Loading policies...</p>';
    
    try {
        const token = localStorage.getItem('brokerToken');
        // TODO: Implement API endpoint for getting broker's policies pending approval
        // const response = await fetch(`${API_BASE_URL}/broker/${brokerId}/policies/pending-approval`, {
        //     headers: { 'Authorization': `Bearer ${token}` }
        // });
        
        // For now, show placeholder
        policiesList.innerHTML = `
            <div class="policy-card">
                <p style="color: var(--text-secondary);">Policies you've sold that are waiting for manager approval will appear here.</p>
            </div>
        `;
    } catch (error) {
        policiesList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading policies</p>';
    }
}

// Load all policies
async function loadAllPolicies(brokerId) {
    const policiesList = document.getElementById('allPoliciesList');
    if (!policiesList) return;
    
    policiesList.innerHTML = '<p class="loading-text">Loading policies...</p>';
    
    try {
        // TODO: Implement API endpoint
        policiesList.innerHTML = `
            <div class="policy-card">
                <p style="color: var(--text-secondary);">All your policies will be displayed here.</p>
            </div>
        `;
    } catch (error) {
        policiesList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading policies</p>';
    }
}

// Load monthly report
async function loadMonthlyReport(brokerId) {
    // TODO: Implement
}

// Load commission
async function loadCommission(brokerId) {
    // TODO: Implement
}

// Load Create Client Form
function loadCreateClientForm() {
    const formContainer = document.getElementById('createClientForm');
    if (!formContainer) return;
    
    formContainer.innerHTML = `
        <form id="newClientForm" class="client-form">
            <div class="form-grid">
                <div class="form-group">
                    <label for="firstName" class="required">First Name</label>
                    <input type="text" id="firstName" name="firstName" required>
                </div>
                <div class="form-group">
                    <label for="lastName" class="required">Last Name</label>
                    <input type="text" id="lastName" name="lastName" required>
                </div>
                <div class="form-group">
                    <label for="email" class="required">Email</label>
                    <input type="email" id="email" name="email" required>
                </div>
                <div class="form-group">
                    <label for="phone">Phone Number</label>
                    <input type="tel" id="phone" name="phone">
                </div>
                <div class="form-group">
                    <label for="idNumber">ID Number</label>
                    <input type="text" id="idNumber" name="idNumber">
                </div>
                <div class="form-group">
                    <label for="dateOfBirth">Date of Birth</label>
                    <input type="date" id="dateOfBirth" name="dateOfBirth">
                </div>
                <div class="form-group full-width">
                    <label for="address">Address</label>
                    <textarea id="address" name="address" rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label for="password" class="required">Password</label>
                    <input type="password" id="password" name="password" required>
                    <small>Client will use this password to log in</small>
                </div>
                <div class="form-group">
                    <label for="confirmPassword" class="required">Confirm Password</label>
                    <input type="password" id="confirmPassword" name="confirmPassword" required>
                </div>
            </div>
            <div class="error-message" id="formError"></div>
            <div class="success-message" id="formSuccess"></div>
            <div class="form-actions">
                <button type="button" class="btn btn-secondary" id="cancelBtn">Cancel</button>
                <button type="submit" class="btn btn-primary" id="submitBtn">Create Client</button>
            </div>
        </form>
    `;
    
    // Add form submission handler
    const form = document.getElementById('newClientForm');
    if (form) {
        form.addEventListener('submit', handleCreateClient);
    }
    
    // Add cancel button handler
    const cancelBtn = document.getElementById('cancelBtn');
    if (cancelBtn) {
        cancelBtn.addEventListener('click', () => {
            form.reset();
            const errorDiv = document.getElementById('formError');
            const successDiv = document.getElementById('formSuccess');
            if (errorDiv) errorDiv.classList.remove('show');
            if (successDiv) successDiv.classList.remove('show');
        });
    }
}

// Handle Create Client Form Submission
async function handleCreateClient(e) {
    e.preventDefault();
    
    const errorDiv = document.getElementById('formError');
    const successDiv = document.getElementById('formSuccess');
    const submitBtn = document.getElementById('submitBtn');
    
    // Clear previous messages
    if (errorDiv) errorDiv.classList.remove('show');
    if (successDiv) successDiv.classList.remove('show');
    
    // Get form values
    const formData = {
        firstName: document.getElementById('firstName').value.trim(),
        lastName: document.getElementById('lastName').value.trim(),
        email: document.getElementById('email').value.trim(),
        phone: document.getElementById('phone').value.trim(),
        idNumber: document.getElementById('idNumber').value.trim(),
        dateOfBirth: document.getElementById('dateOfBirth').value,
        address: document.getElementById('address').value.trim(),
        password: document.getElementById('password').value,
        confirmPassword: document.getElementById('confirmPassword').value
    };
    
    // Validation
    if (formData.password !== formData.confirmPassword) {
        if (errorDiv) {
            errorDiv.textContent = 'Passwords do not match';
            errorDiv.classList.add('show');
        }
        return;
    }
    
    if (formData.password.length < 6) {
        if (errorDiv) {
            errorDiv.textContent = 'Password must be at least 6 characters';
            errorDiv.classList.add('show');
        }
        return;
    }
    
    // Disable submit button
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating...';
    }
    
    try {
        const token = localStorage.getItem('brokerToken');
        const brokerInfo = JSON.parse(localStorage.getItem('brokerInfo'));
        
        // TODO: Replace with actual API endpoint when available
        // For now, show success message
        const response = await fetch(`${API_BASE_URL}/policyholder/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                firstName: formData.firstName,
                lastName: formData.lastName,
                email: formData.email,
                phone: formData.phone || null,
                idNumber: formData.idNumber || null,
                dateOfBirth: formData.dateOfBirth || null,
                address: formData.address || null,
                password: formData.password,
                brokerId: brokerInfo?.id
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            if (successDiv) {
                successDiv.textContent = `Client created successfully! Policy Number: ${data.policyNumber || 'N/A'}`;
                successDiv.classList.add('show');
            }
            document.getElementById('newClientForm').reset();
        } else {
            if (errorDiv) {
                errorDiv.textContent = data.message || 'Failed to create client. Please try again.';
                errorDiv.classList.add('show');
            }
        }
    } catch (error) {
        console.error('Error creating client:', error);
        if (errorDiv) {
            errorDiv.textContent = 'Connection error. Please check your internet connection and try again.';
            errorDiv.classList.add('show');
        }
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Create Client';
        }
    }
}

// Initialize portal
document.addEventListener('DOMContentLoaded', () => {
    if (!checkAuth()) {
        return;
    }
    
    loadBrokerInfo();
    initNavigation();
    initLogout();
    
    // Load initial section data (default to Pending Approval)
    const activeSection = document.querySelector('.nav-btn.active')?.dataset.section || 'new-clients';
    loadSectionData(activeSection);
});
