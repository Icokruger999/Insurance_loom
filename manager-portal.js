// Manager Portal JavaScript

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
    const token = localStorage.getItem('managerToken');
    const managerInfo = localStorage.getItem('managerInfo');
    
    if (!token || !managerInfo) {
        // Not logged in, redirect to home page
        window.location.href = '/';
        return false;
    }
    
    return true;
}

// Load manager information
function loadManagerInfo() {
    const managerInfo = localStorage.getItem('managerInfo');
    if (managerInfo) {
        try {
            const manager = JSON.parse(managerInfo);
            const managerNameEl = document.getElementById('managerName');
            const managerEmailEl = document.getElementById('managerEmail');
            
            if (managerNameEl) {
                managerNameEl.textContent = `${manager.firstName || ''} ${manager.lastName || ''}`.trim() || 'Manager';
            }
            
            if (managerEmailEl) {
                managerEmailEl.textContent = manager.email || '';
            }
        } catch (error) {
            console.error('Error parsing manager info:', error);
        }
    }
}

// Navigation handling
function initNavigation() {
    const navButtons = document.querySelectorAll('.nav-btn');
    const sections = document.querySelectorAll('.content-section');
    const sectionTitle = document.getElementById('sectionTitle');
    
    const sectionTitles = {
        'agents': 'Agents by Region',
        'pending-policies': 'Pending Policies',
        'approvals': 'Approval History',
        'reports': 'Reports & Analytics'
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
            localStorage.removeItem('managerToken');
            localStorage.removeItem('managerInfo');
            window.location.href = '/';
        });
    }
}

// Load section-specific data
async function loadSectionData(sectionId) {
    const token = localStorage.getItem('managerToken');
    if (!token) return;
    
    try {
        switch (sectionId) {
            case 'agents':
                await loadAgents();
                break;
            case 'pending-policies':
                await loadPendingPolicies();
                break;
            case 'approvals':
                await loadApprovalHistory();
                break;
            case 'reports':
                await loadReports();
                break;
        }
    } catch (error) {
        console.error(`Error loading ${sectionId}:`, error);
    }
}

// Load agents by region
async function loadAgents() {
    const agentsList = document.getElementById('agentsList');
    if (!agentsList) return;
    
    agentsList.innerHTML = '<p class="loading-text">Loading agents...</p>';
    
    try {
        // TODO: Implement API endpoint for getting agents by region
        // const response = await fetch(`${API_BASE_URL}/manager/agents?region=${region}`, {
        //     headers: { 'Authorization': `Bearer ${token}` }
        // });
        
        // For now, show placeholder
        agentsList.innerHTML = `
            <div class="agent-card">
                <div class="card-header">
                    <div class="card-title">Agents List</div>
                </div>
                <p style="color: var(--text-secondary);">Agent listing by region will be displayed here.</p>
            </div>
        `;
    } catch (error) {
        agentsList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading agents</p>';
    }
}

// Load pending policies
async function loadPendingPolicies() {
    const policiesList = document.getElementById('pendingPoliciesList');
    if (!policiesList) return;
    
    policiesList.innerHTML = '<p class="loading-text">Loading pending policies...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        // TODO: Implement API endpoint for getting pending policies
        // const response = await fetch(`${API_BASE_URL}/manager/pending-policies`, {
        //     headers: { 'Authorization': `Bearer ${token}` }
        // });
        
        // For now, show placeholder
        policiesList.innerHTML = `
            <div class="policy-card">
                <div class="card-header">
                    <div class="card-title">Pending Policies</div>
                    <span class="card-status status-pending">Pending</span>
                </div>
                <p style="color: var(--text-secondary);">Policies requiring approval will be displayed here.</p>
            </div>
        `;
    } catch (error) {
        policiesList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading policies</p>';
    }
}

// Load approval history
async function loadApprovalHistory() {
    const approvalList = document.getElementById('approvalHistoryList');
    if (!approvalList) return;
    
    approvalList.innerHTML = '<p class="loading-text">Loading approval history...</p>';
    
    try {
        // TODO: Implement API endpoint for approval history
        approvalList.innerHTML = `
            <div class="approval-card">
                <div class="card-header">
                    <div class="card-title">Approval History</div>
                </div>
                <p style="color: var(--text-secondary);">Your approval and rejection history will be displayed here.</p>
            </div>
        `;
    } catch (error) {
        approvalList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading history</p>';
    }
}

// Load reports
async function loadReports() {
    const reportsContent = document.getElementById('reportsContent');
    if (!reportsContent) return;
    
    reportsContent.innerHTML = '<p class="loading-text">Loading reports...</p>';
    
    try {
        // TODO: Implement API endpoint for reports
        reportsContent.innerHTML = `
            <div class="reports-content">
                <p style="color: var(--text-secondary);">Reports and analytics will be displayed here.</p>
            </div>
        `;
    } catch (error) {
        reportsContent.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading reports</p>';
    }
}

// Initialize portal
document.addEventListener('DOMContentLoaded', () => {
    if (!checkAuth()) {
        return;
    }
    
    loadManagerInfo();
    initNavigation();
    initLogout();
    
    // Load initial section data
    loadSectionData('agents');
});

