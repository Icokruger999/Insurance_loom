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
        'dashboard': 'Dashboard',
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
            case 'dashboard':
                await loadDashboard();
                break;
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

// Load Dashboard
async function loadDashboard() {
    const dashboardContent = document.getElementById('dashboardContent');
    if (!dashboardContent) return;
    
    dashboardContent.innerHTML = '<p class="loading-text">Loading dashboard...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        
        // Fetch pending applications for statistics
        const pendingResponse = await fetch(`${API_BASE_URL}/policy-approval/pending`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        const approvedResponse = await fetch(`${API_BASE_URL}/policy-approval/approved`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        const rejectedResponse = await fetch(`${API_BASE_URL}/policy-approval/rejected`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        const pendingApps = pendingResponse.ok ? await pendingResponse.json() : [];
        const approvedApps = approvedResponse.ok ? await approvedResponse.json() : [];
        const rejectedApps = rejectedResponse.ok ? await rejectedResponse.json() : [];
        
        const totalPolicies = pendingApps.length + approvedApps.length + rejectedApps.length;
        const activePolicies = approvedApps.length;
        const pendingPolicies = pendingApps.length;
        const rejectedPolicies = rejectedApps.length;
        
        // South African regions
        const regions = [
            'Cape Town', 'Johannesburg', 'Pretoria', 'Durban', 
            'Port Elizabeth', 'Bloemfontein', 'Free State', 'Limpopo',
            'Mpumalanga', 'North West', 'Northern Cape', 'Eastern Cape',
            'Western Cape', 'KwaZulu-Natal', 'Gauteng'
        ];
        
        // For now, we'll use mock data for regions - this should come from API later
        const regionData = regions.map(region => ({
            name: region,
            policies: Math.floor(Math.random() * 50) + 10, // Mock data
            premium: Math.floor(Math.random() * 100000) + 50000
        }));
        
        dashboardContent.innerHTML = `
            <div class="dashboard-container" style="background: #f0f8f4; padding: 1.5rem; border-radius: 8px; min-height: 100vh;">
                <!-- Filters Section -->
                <div id="dashboardFilters" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; margin-bottom: 2rem; padding: 1.5rem; background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                    <div>
                        <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">Region</label>
                        <select id="dashboardFilterRegion" onchange="applyDashboardFilters()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                            <option value="">All Regions</option>
                            <option value="Cape Town">Cape Town</option>
                            <option value="Johannesburg">Johannesburg</option>
                            <option value="Pretoria">Pretoria</option>
                            <option value="Durban">Durban</option>
                            <option value="Bloemfontein">Bloemfontein</option>
                            <option value="Free State">Free State</option>
                            <option value="Western Cape">Western Cape</option>
                            <option value="Gauteng">Gauteng</option>
                            <option value="KwaZulu-Natal">KwaZulu-Natal</option>
                        </select>
                    </div>
                    <div>
                        <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">Broker</label>
                        <select id="dashboardFilterBroker" onchange="applyDashboardFilters()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                            <option value="">All Brokers</option>
                        </select>
                    </div>
                    <div>
                        <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">Status</label>
                        <select id="dashboardFilterStatus" onchange="applyDashboardFilters()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                            <option value="">All Statuses</option>
                            <option value="Draft">Draft</option>
                            <option value="PendingSubmission">Pending Submission</option>
                            <option value="Submitted">Submitted</option>
                            <option value="UnderReview">Under Review</option>
                            <option value="Approved">Approved</option>
                            <option value="Active">Active</option>
                            <option value="Rejected">Rejected</option>
                            <option value="Cancelled">Cancelled</option>
                            <option value="ChangesRequired">Changes Required</option>
                        </select>
                    </div>
                    <div>
                        <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">From Date</label>
                        <input type="date" id="dashboardFilterStartDate" onchange="applyDashboardFilters()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                    </div>
                    <div>
                        <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">To Date</label>
                        <input type="date" id="dashboardFilterEndDate" onchange="applyDashboardFilters()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                    </div>
                </div>
                
                <!-- Key Performance Indicators -->
                <div class="dashboard-kpis" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1.5rem; margin-bottom: 2rem;">
                    <div class="kpi-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Total Policies</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--primary-color);">${totalPolicies}</div>
                    </div>
                    <div class="kpi-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Active Policies</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--success-color);">${activePolicies}</div>
                    </div>
                    <div class="kpi-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Pending Approval</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--warning-color);">${pendingPolicies}</div>
                    </div>
                    <div class="kpi-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Rejected</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--danger-color);">${rejectedPolicies}</div>
                    </div>
                </div>
                
                <!-- Charts Row -->
                <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; margin-bottom: 2rem;">
                    <!-- Business Overview by Region -->
                    <div class="chart-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Business Overview by Region</h3>
                        <div style="height: 300px; display: flex; align-items: flex-end; gap: 0.5rem; border-bottom: 2px solid var(--border-color); padding-bottom: 1rem;">
                            ${regionData.slice(0, 8).map(region => {
                                const maxPolicies = Math.max(...regionData.map(r => r.policies));
                                const height = (region.policies / maxPolicies) * 100;
                                return `
                                    <div style="flex: 1; display: flex; flex-direction: column; align-items: center;">
                                        <div style="width: 100%; background: linear-gradient(180deg, var(--primary-color) 0%, var(--primary-hover) 100%); height: ${height}%; min-height: 20px; border-radius: 4px 4px 0 0; margin-bottom: 0.5rem;"></div>
                                        <div style="font-size: 0.75rem; color: var(--text-muted); text-align: center; transform: rotate(-45deg); transform-origin: center; white-space: nowrap;">${region.name}</div>
                                    </div>
                                `;
                            }).join('')}
                        </div>
                    </div>
                    
                    <!-- Policy Volume Comparison -->
                    <div class="chart-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Policy Volume</h3>
                        <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 250px;">
                            <div style="width: 150px; height: 150px; border-radius: 50%; background: conic-gradient(
                                var(--primary-color) 0% ${(activePolicies / totalPolicies) * 100}%,
                                var(--warning-color) ${(activePolicies / totalPolicies) * 100}% ${((activePolicies + pendingPolicies) / totalPolicies) * 100}%,
                                var(--danger-color) ${((activePolicies + pendingPolicies) / totalPolicies) * 100}% 100%
                            ); display: flex; align-items: center; justify-content: center; margin-bottom: 1rem;">
                                <div style="background: white; width: 80px; height: 80px; border-radius: 50%; display: flex; align-items: center; justify-content: center;">
                                    <div style="font-size: 1.5rem; font-weight: 700; color: var(--text-primary);">${totalPolicies}</div>
                                </div>
                            </div>
                            <div style="display: flex; flex-direction: column; gap: 0.5rem; width: 100%;">
                                <div style="display: flex; align-items: center; gap: 0.5rem;">
                                    <div style="width: 12px; height: 12px; background: var(--primary-color); border-radius: 2px;"></div>
                                    <span style="font-size: 0.875rem; color: var(--text-secondary);">Active: ${activePolicies} (${totalPolicies > 0 ? Math.round((activePolicies / totalPolicies) * 100) : 0}%)</span>
                                </div>
                                <div style="display: flex; align-items: center; gap: 0.5rem;">
                                    <div style="width: 12px; height: 12px; background: var(--warning-color); border-radius: 2px;"></div>
                                    <span style="font-size: 0.875rem; color: var(--text-secondary);">Pending: ${pendingPolicies} (${totalPolicies > 0 ? Math.round((pendingPolicies / totalPolicies) * 100) : 0}%)</span>
                                </div>
                                <div style="display: flex; align-items: center; gap: 0.5rem;">
                                    <div style="width: 12px; height: 12px; background: var(--danger-color); border-radius: 2px;"></div>
                                    <span style="font-size: 0.875rem; color: var(--text-secondary);">Rejected: ${rejectedPolicies} (${totalPolicies > 0 ? Math.round((rejectedPolicies / totalPolicies) * 100) : 0}%)</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- All Policies Table with Filters -->
                <div class="table-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
                        <h3 style="margin: 0; color: var(--text-primary); font-size: 1.25rem;">All Policies</h3>
                        <button onclick="loadAllPoliciesTable()" style="padding: 0.5rem 1rem; background: var(--primary-color); color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 0.875rem;">Refresh</button>
                    </div>
                    <div id="policiesFilters" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; padding: 1rem; background: var(--bg-secondary); border-radius: 8px;">
                        <div>
                            <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">Region</label>
                            <select id="filterRegion" onchange="filterPolicies()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                                <option value="">All Regions</option>
                                <option value="Cape Town">Cape Town</option>
                                <option value="Johannesburg">Johannesburg</option>
                                <option value="Pretoria">Pretoria</option>
                                <option value="Durban">Durban</option>
                                <option value="Bloemfontein">Bloemfontein</option>
                                <option value="Free State">Free State</option>
                                <option value="Western Cape">Western Cape</option>
                                <option value="Gauteng">Gauteng</option>
                                <option value="KwaZulu-Natal">KwaZulu-Natal</option>
                            </select>
                        </div>
                        <div>
                            <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">Broker</label>
                            <select id="filterBroker" onchange="filterPolicies()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                                <option value="">All Brokers</option>
                            </select>
                        </div>
                        <div>
                            <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">Status</label>
                            <select id="filterStatus" onchange="filterPolicies()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                                <option value="">All Statuses</option>
                                <option value="Draft">Draft</option>
                                <option value="PendingSubmission">Pending Submission</option>
                                <option value="Submitted">Submitted</option>
                                <option value="UnderReview">Under Review</option>
                                <option value="Approved">Approved</option>
                                <option value="Active">Active</option>
                                <option value="Rejected">Rejected</option>
                                <option value="Cancelled">Cancelled</option>
                                <option value="ChangesRequired">Changes Required</option>
                            </select>
                        </div>
                        <div>
                            <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">From Date</label>
                            <input type="date" id="filterStartDate" onchange="filterPolicies()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                        </div>
                        <div>
                            <label style="display: block; margin-bottom: 0.5rem; font-size: 0.875rem; color: var(--text-muted); font-weight: 500;">To Date</label>
                            <input type="date" id="filterEndDate" onchange="filterPolicies()" style="width: 100%; padding: 0.5rem; border: 1px solid var(--border-color); border-radius: 4px; font-size: 0.875rem;">
                        </div>
                    </div>
                    <div id="allPoliciesTable">
                        <p class="loading-text">Loading policies...</p>
                    </div>
                </div>
            </div>
        `;
    } catch (error) {
        console.error('Error loading dashboard:', error);
        dashboardContent.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading dashboard</p>';
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
    loadSectionData('dashboard');
});

