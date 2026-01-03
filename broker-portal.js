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

// Initialize portal
document.addEventListener('DOMContentLoaded', () => {
    if (!checkAuth()) {
        return;
    }
    
    loadBrokerInfo();
    initNavigation();
    initLogout();
    
    // Load initial section data
    loadSectionData('new-clients');
});
