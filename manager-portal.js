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
        'pending-applications': 'Pending Applications',
        'approved-applications': 'Approved Applications',
        'rejected-applications': 'Rejected Applications',
        'agents': 'Agents by Region',
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
            case 'pending-applications':
                await loadPendingApplications();
                break;
            case 'approved-applications':
                await loadApprovedApplications();
                break;
            case 'rejected-applications':
                await loadRejectedApplications();
                break;
            case 'agents':
                await loadAgents();
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

// Load Pending Applications
async function loadPendingApplications() {
    const applicationsList = document.getElementById('pendingApplicationsList');
    if (!applicationsList) return;
    
    applicationsList.innerHTML = '<p class="loading-text">Loading pending applications...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        const response = await fetch(`${API_BASE_URL}/policy-approval/pending`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) {
            throw new Error('Failed to load pending applications');
        }
        
        const applications = await response.json();
        
        if (applications.length === 0) {
            applicationsList.innerHTML = `
                <div class="application-card">
                    <p style="color: var(--text-secondary);">No pending applications.</p>
                </div>
            `;
            return;
        }
        
        applicationsList.innerHTML = applications.map(app => `
            <div class="application-card" style="margin-bottom: 1rem; padding: 1.5rem; background-color: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 1rem;">
                    <div>
                        <h4 style="margin: 0 0 0.5rem 0; color: var(--text-primary);">${app.policyNumber}</h4>
                        <p style="margin: 0; color: var(--text-secondary);"><strong>Policy Holder:</strong> ${app.policyHolderName}</p>
                        <p style="margin: 0.5rem 0; color: var(--text-secondary);"><strong>Broker:</strong> ${app.brokerName}</p>
                        <p style="margin: 0.5rem 0; color: var(--text-secondary); font-size: 0.9rem;">${app.serviceType}</p>
                        <p style="margin: 0.5rem 0 0 0; color: var(--text-muted); font-size: 0.875rem;">
                            Submitted: ${new Date(app.submittedDate).toLocaleDateString()}
                        </p>
                    </div>
                    <span class="status-badge PendingSubmission" style="padding: 0.375rem 0.75rem; border-radius: 4px; font-size: 0.875rem; font-weight: 500; background-color: #ffc107; color: #000;">
                        ${app.status}
                    </span>
                </div>
                <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; margin-bottom: 1rem;">
                    <div>
                        <p style="margin: 0; font-size: 0.875rem; color: var(--text-muted);">Coverage Amount</p>
                        <p style="margin: 0.25rem 0 0 0; color: var(--text-primary); font-weight: 500;">R ${app.coverageAmount?.toFixed(2) || '0.00'}</p>
                    </div>
                    <div>
                        <p style="margin: 0; font-size: 0.875rem; color: var(--text-muted);">Premium Amount</p>
                        <p style="margin: 0.25rem 0 0 0; color: var(--text-primary); font-weight: 500;">R ${app.premiumAmount?.toFixed(2) || '0.00'}</p>
                    </div>
                </div>
                <div style="display: flex; gap: 0.5rem; margin-top: 1rem;">
                    <button class="btn btn-primary" onclick="approveApplication('${app.policyId}')" style="padding: 0.5rem 1rem; font-size: 0.875rem;">
                        Approve
                    </button>
                    <button class="btn btn-secondary" onclick="showRejectModal('${app.policyId}', '${app.policyNumber}')" style="padding: 0.5rem 1rem; font-size: 0.875rem; background-color: var(--danger-color); color: white;">
                        Reject
                    </button>
                    <button class="btn btn-secondary" onclick="viewApplicationDetails('${app.policyId}')" style="padding: 0.5rem 1rem; font-size: 0.875rem;">
                        View Details
                    </button>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading pending applications:', error);
        applicationsList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading pending applications</p>';
    }
}

// Load Approved Applications
async function loadApprovedApplications() {
    const applicationsList = document.getElementById('approvedApplicationsList');
    if (!applicationsList) return;
    
    applicationsList.innerHTML = '<p class="loading-text">Loading approved applications...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        const response = await fetch(`${API_BASE_URL}/policy-approval/approved`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) {
            throw new Error('Failed to load approved applications');
        }
        
        const applications = await response.json();
        
        if (applications.length === 0) {
            applicationsList.innerHTML = `
                <div class="application-card">
                    <p style="color: var(--text-secondary);">No approved applications.</p>
                </div>
            `;
            return;
        }
        
        applicationsList.innerHTML = applications.map(app => `
            <div class="application-card" style="margin-bottom: 1rem; padding: 1.5rem; background-color: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 1rem;">
                    <div>
                        <h4 style="margin: 0 0 0.5rem 0; color: var(--text-primary);">${app.policyNumber}</h4>
                        <p style="margin: 0; color: var(--text-secondary);"><strong>Policy Holder:</strong> ${app.policyHolderName}</p>
                        <p style="margin: 0.5rem 0; color: var(--text-secondary);"><strong>Broker:</strong> ${app.brokerName}</p>
                        <p style="margin: 0.5rem 0; color: var(--text-secondary); font-size: 0.9rem;">${app.serviceType}</p>
                        <p style="margin: 0.5rem 0 0 0; color: var(--text-muted); font-size: 0.875rem;">
                            Approved: ${app.approvedDate ? new Date(app.approvedDate).toLocaleDateString() : 'N/A'}
                        </p>
                    </div>
                    <span class="status-badge Approved" style="padding: 0.375rem 0.75rem; border-radius: 4px; font-size: 0.875rem; font-weight: 500; background-color: #28a745; color: white;">
                        Approved
                    </span>
                </div>
                <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem;">
                    <div>
                        <p style="margin: 0; font-size: 0.875rem; color: var(--text-muted);">Coverage Amount</p>
                        <p style="margin: 0.25rem 0 0 0; color: var(--text-primary); font-weight: 500;">R ${app.coverageAmount?.toFixed(2) || '0.00'}</p>
                    </div>
                    <div>
                        <p style="margin: 0; font-size: 0.875rem; color: var(--text-muted);">Premium Amount</p>
                        <p style="margin: 0.25rem 0 0 0; color: var(--text-primary); font-weight: 500;">R ${app.premiumAmount?.toFixed(2) || '0.00'}</p>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading approved applications:', error);
        applicationsList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading approved applications</p>';
    }
}

// Load Rejected Applications
async function loadRejectedApplications() {
    const applicationsList = document.getElementById('rejectedApplicationsList');
    if (!applicationsList) return;
    
    applicationsList.innerHTML = '<p class="loading-text">Loading rejected applications...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        const response = await fetch(`${API_BASE_URL}/policy-approval/rejected`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) {
            throw new Error('Failed to load rejected applications');
        }
        
        const applications = await response.json();
        
        if (applications.length === 0) {
            applicationsList.innerHTML = `
                <div class="application-card">
                    <p style="color: var(--text-secondary);">No rejected applications.</p>
                </div>
            `;
            return;
        }
        
        applicationsList.innerHTML = applications.map(app => `
            <div class="application-card" style="margin-bottom: 1rem; padding: 1.5rem; background-color: var(--bg-secondary); border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 1rem;">
                    <div>
                        <h4 style="margin: 0 0 0.5rem 0; color: var(--text-primary);">${app.policyNumber}</h4>
                        <p style="margin: 0; color: var(--text-secondary);"><strong>Policy Holder:</strong> ${app.policyHolderName}</p>
                        <p style="margin: 0.5rem 0; color: var(--text-secondary);"><strong>Broker:</strong> ${app.brokerName}</p>
                        <p style="margin: 0.5rem 0; color: var(--text-secondary); font-size: 0.9rem;">${app.serviceType}</p>
                        <p style="margin: 0.5rem 0 0 0; color: var(--text-muted); font-size: 0.875rem;">
                            Rejected: ${app.rejectedDate ? new Date(app.rejectedDate).toLocaleDateString() : 'N/A'}
                        </p>
                        ${app.rejectionReason ? `
                            <div style="margin-top: 0.5rem; padding: 0.75rem; background-color: rgba(220, 53, 69, 0.1); border-left: 3px solid var(--danger-color); border-radius: 4px;">
                                <p style="margin: 0; font-size: 0.875rem; color: var(--text-secondary);"><strong>Reason:</strong> ${app.rejectionReason}</p>
                            </div>
                        ` : ''}
                    </div>
                    <span class="status-badge Rejected" style="padding: 0.375rem 0.75rem; border-radius: 4px; font-size: 0.875rem; font-weight: 500; background-color: #dc3545; color: white;">
                        Rejected
                    </span>
                </div>
                <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem;">
                    <div>
                        <p style="margin: 0; font-size: 0.875rem; color: var(--text-muted);">Coverage Amount</p>
                        <p style="margin: 0.25rem 0 0 0; color: var(--text-primary); font-weight: 500;">R ${app.coverageAmount?.toFixed(2) || '0.00'}</p>
                    </div>
                    <div>
                        <p style="margin: 0; font-size: 0.875rem; color: var(--text-muted);">Premium Amount</p>
                        <p style="margin: 0.25rem 0 0 0; color: var(--text-primary); font-weight: 500;">R ${app.premiumAmount?.toFixed(2) || '0.00'}</p>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error loading rejected applications:', error);
        applicationsList.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading rejected applications</p>';
    }
}

// Approve Application
async function approveApplication(policyId) {
    if (!confirm('Are you sure you want to approve this application?')) {
        return;
    }
    
    try {
        const token = localStorage.getItem('managerToken');
        const response = await fetch(`${API_BASE_URL}/policy-approval/${policyId}/approve`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({})
        });
        
        const data = await response.json();
        
        if (response.ok) {
            alert('Application approved successfully!');
            // Reload applications
            await loadPendingApplications();
            await loadApprovedApplications();
        } else {
            alert(data.message || 'Failed to approve application');
        }
    } catch (error) {
        console.error('Error approving application:', error);
        alert('Error approving application. Please try again.');
    }
}

// Show Reject Modal
function showRejectModal(policyId, policyNumber) {
    const reason = prompt(`Enter rejection reason for policy ${policyNumber}:`);
    if (reason && reason.trim()) {
        rejectApplication(policyId, reason.trim());
    }
}

// Reject Application
async function rejectApplication(policyId, reason) {
    try {
        const token = localStorage.getItem('managerToken');
        const response = await fetch(`${API_BASE_URL}/policy-approval/${policyId}/reject`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ reason: reason })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            alert('Application rejected successfully!');
            // Reload applications
            await loadPendingApplications();
            await loadRejectedApplications();
        } else {
            alert(data.message || 'Failed to reject application');
        }
    } catch (error) {
        console.error('Error rejecting application:', error);
        alert('Error rejecting application. Please try again.');
    }
}

// View Application Details (placeholder)
function viewApplicationDetails(policyId) {
    alert('Application details view coming soon.');
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
    loadSectionData('pending-applications');
});

