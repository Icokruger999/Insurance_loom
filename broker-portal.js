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
async function loadCreateClientForm() {
    const formContainer = document.getElementById('createClientForm');
    if (!formContainer) return;
    
    formContainer.innerHTML = '<p class="loading-text">Loading form...</p>';
    
    try {
        // Load service types
        const serviceTypesResponse = await fetch(`${API_BASE_URL}/servicetypes?activeOnly=true`);
        const serviceTypes = serviceTypesResponse.ok ? await serviceTypesResponse.json() : [];
        
        formContainer.innerHTML = `
            <form id="newClientForm" class="client-form">
                <h3 style="margin-bottom: 2rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Client Information</h3>
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
                        <label for="idNumber" class="required">ID Number</label>
                        <input type="text" id="idNumber" name="idNumber" required maxlength="13">
                        <small>South African ID number (13 digits)</small>
                    </div>
                    <div class="form-group">
                        <label for="dateOfBirth" class="required">Date of Birth</label>
                        <input type="date" id="dateOfBirth" name="dateOfBirth" required>
                    </div>
                    <div class="form-group">
                        <label for="email" class="required">Email Address</label>
                        <input type="email" id="email" name="email" required>
                    </div>
                    <div class="form-group">
                        <label for="phone" class="required">Phone Number</label>
                        <input type="tel" id="phone" name="phone" required>
                        <small>Include country code if international</small>
                    </div>
                    <div class="form-group full-width">
                        <label for="address" class="required">Physical Address</label>
                        <textarea id="address" name="address" rows="3" required></textarea>
                    </div>
                </div>
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Insurance Product Selection</h3>
                <div class="form-grid">
                    <div class="form-group full-width">
                        <label for="serviceType" class="required">Select Insurance Product</label>
                        <select id="serviceType" name="serviceType" required>
                            <option value="">-- Select a product --</option>
                            ${serviceTypes.map(st => `<option value="${st.id}">${st.serviceName}${st.description ? ' - ' + st.description : ''}</option>`).join('')}
                        </select>
                        <small>Choose the insurance product the client wants to purchase</small>
                    </div>
                    <div class="form-group">
                        <label for="coverageAmount">Coverage Amount (R)</label>
                        <input type="number" id="coverageAmount" name="coverageAmount" min="0" step="0.01">
                        <small>Total coverage amount</small>
                    </div>
                    <div class="form-group">
                        <label for="premiumAmount">Premium Amount (R)</label>
                        <input type="number" id="premiumAmount" name="premiumAmount" min="0" step="0.01">
                        <small>Monthly premium amount</small>
                    </div>
                    <div class="form-group">
                        <label for="activationDate" class="required">Policy Activation Date</label>
                        <input type="date" id="activationDate" name="activationDate" required min="${new Date().toISOString().split('T')[0]}">
                        <small>When should the policy become active?</small>
                    </div>
                </div>
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Financial Information</h3>
                <div class="form-grid">
                    <div class="form-group">
                        <label for="monthlyIncome" class="required">Monthly Income (R)</label>
                        <input type="number" id="monthlyIncome" name="monthlyIncome" required min="0" step="0.01">
                        <small>Client's monthly income</small>
                    </div>
                    <div class="form-group">
                        <label for="monthlyExpenses" class="required">Monthly Expenses (R)</label>
                        <input type="number" id="monthlyExpenses" name="monthlyExpenses" required min="0" step="0.01">
                        <small>Client's monthly expenses</small>
                    </div>
                    <div class="form-group">
                        <label for="paymentDate" class="required">Preferred Payment Date</label>
                        <select id="paymentDate" name="paymentDate" required>
                            <option value="">-- Select day --</option>
                            ${Array.from({length: 28}, (_, i) => i + 1).map(day => `<option value="${day}">${day}${day === 1 ? 'st' : day === 2 ? 'nd' : day === 3 ? 'rd' : 'th'}</option>`).join('')}
                        </select>
                        <small>Day of month for premium payment</small>
                    </div>
                </div>
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Required Documents</h3>
                <div class="form-grid">
                    <div class="form-group">
                        <label for="idDocument" class="required">ID Document</label>
                        <input type="file" id="idDocument" name="idDocument" accept=".pdf,.jpg,.jpeg,.png" required>
                        <small>Upload a copy of ID document (PDF, JPG, PNG - Max 5MB)</small>
                    </div>
                    <div class="form-group">
                        <label for="proofOfAddress" class="required">Proof of Address</label>
                        <input type="file" id="proofOfAddress" name="proofOfAddress" accept=".pdf,.jpg,.jpeg,.png" required>
                        <small>Upload proof of address (PDF, JPG, PNG - Max 5MB)</small>
                    </div>
                    <div class="form-group">
                        <label for="bankStatement">Bank Statement (Optional)</label>
                        <input type="file" id="bankStatement" name="bankStatement" accept=".pdf,.jpg,.jpeg,.png">
                        <small>Upload recent bank statement if available</small>
                    </div>
                </div>
                
                <div class="error-message" id="formError"></div>
                <div class="success-message" id="formSuccess"></div>
                <div class="form-actions">
                    <button type="button" class="btn btn-secondary" id="cancelBtn">Cancel</button>
                    <button type="submit" class="btn btn-primary" id="submitBtn">Create Client & Policy</button>
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
    } catch (error) {
        console.error('Error loading form:', error);
        formContainer.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading form. Please refresh the page.</p>';
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
        serviceTypeId: document.getElementById('serviceType').value || null,
        coverageAmount: document.getElementById('coverageAmount').value ? parseFloat(document.getElementById('coverageAmount').value) : null,
        premiumAmount: document.getElementById('premiumAmount').value ? parseFloat(document.getElementById('premiumAmount').value) : null,
        activationDate: document.getElementById('activationDate').value || null,
        monthlyIncome: document.getElementById('monthlyIncome').value ? parseFloat(document.getElementById('monthlyIncome').value) : null,
        monthlyExpenses: document.getElementById('monthlyExpenses').value ? parseFloat(document.getElementById('monthlyExpenses').value) : null,
        paymentDate: document.getElementById('paymentDate').value ? parseInt(document.getElementById('paymentDate').value) : null,
        idDocument: document.getElementById('idDocument').files[0],
        proofOfAddress: document.getElementById('proofOfAddress').files[0],
        bankStatement: document.getElementById('bankStatement').files[0] || null
    };
    
    // Validation
    if (!formData.serviceTypeId) {
        if (errorDiv) {
            errorDiv.textContent = 'Please select an insurance product';
            errorDiv.classList.add('show');
        }
        return;
    }
    
    if (!formData.idDocument || !formData.proofOfAddress) {
        if (errorDiv) {
            errorDiv.textContent = 'Please upload required documents (ID and Proof of Address)';
            errorDiv.classList.add('show');
        }
        return;
    }
    
    // Validate file sizes (5MB max)
    const maxFileSize = 5 * 1024 * 1024; // 5MB in bytes
    if (formData.idDocument.size > maxFileSize) {
        if (errorDiv) {
            errorDiv.textContent = 'ID Document file size exceeds 5MB limit';
            errorDiv.classList.add('show');
        }
        return;
    }
    if (formData.proofOfAddress.size > maxFileSize) {
        if (errorDiv) {
            errorDiv.textContent = 'Proof of Address file size exceeds 5MB limit';
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
        
        // Create FormData for file uploads
        const formDataToSend = new FormData();
        formDataToSend.append('firstName', formData.firstName);
        formDataToSend.append('lastName', formData.lastName);
        formDataToSend.append('email', formData.email);
        formDataToSend.append('phone', formData.phone || '');
        formDataToSend.append('idNumber', formData.idNumber || '');
        formDataToSend.append('dateOfBirth', formData.dateOfBirth || '');
        formDataToSend.append('address', formData.address || '');
        formDataToSend.append('serviceTypeId', formData.serviceTypeId || '');
        if (formData.coverageAmount) formDataToSend.append('coverageAmount', formData.coverageAmount.toString());
        if (formData.premiumAmount) formDataToSend.append('premiumAmount', formData.premiumAmount.toString());
        formDataToSend.append('activationDate', formData.activationDate || '');
        if (formData.monthlyIncome) formDataToSend.append('monthlyIncome', formData.monthlyIncome.toString());
        if (formData.monthlyExpenses) formDataToSend.append('monthlyExpenses', formData.monthlyExpenses.toString());
        if (formData.paymentDate) formDataToSend.append('paymentDate', formData.paymentDate.toString());
        formDataToSend.append('idDocument', formData.idDocument);
        formDataToSend.append('proofOfAddress', formData.proofOfAddress);
        if (formData.bankStatement) formDataToSend.append('bankStatement', formData.bankStatement);
        
        const response = await fetch(`${API_BASE_URL}/policyholder/register`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formDataToSend
        });
        
        const data = await response.json();
        
        if (response.ok) {
            if (successDiv) {
                let successMsg = `Client created successfully!`;
                if (data.policyNumber) {
                    successMsg += `\nPolicy Holder Number: ${data.policyNumber}`;
                }
                if (data.policyNumberGenerated) {
                    successMsg += `\nPolicy Number: ${data.policyNumberGenerated}`;
                }
                successDiv.textContent = successMsg;
                successDiv.classList.add('show');
            }
            document.getElementById('newClientForm').reset();
            
            // Scroll to success message
            if (successDiv) {
                successDiv.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }
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
    // Check auth first before rendering anything
    if (!checkAuth()) {
        return;
    }
    
    // Show portal once authenticated
    const container = document.querySelector('.portal-container');
    if (container) {
        container.classList.add('loaded');
    }
    
    loadBrokerInfo();
    initNavigation();
    initLogout();
    
    // Load initial section data (default to Pending Approval)
    const activeSection = document.querySelector('.nav-btn.active')?.dataset.section || 'new-clients';
    loadSectionData(activeSection);
});
