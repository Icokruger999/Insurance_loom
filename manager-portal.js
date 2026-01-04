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
            case 'broker-activity':
                await loadBrokerActivity();
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
                <!-- Dashboard Header with Refresh Button -->
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
                    <h2 style="margin: 0; color: var(--text-primary);">Dashboard Overview</h2>
                    <button onclick="refreshDashboard()" id="refreshDashboardBtn" style="padding: 0.5rem 1rem; background: var(--primary-color); color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 0.875rem; display: flex; align-items: center; gap: 0.5rem; transition: all 0.3s;">
                        <span id="refreshIcon">🔄</span> Refresh
                    </button>
                </div>
                <!-- Key Performance Indicators -->
                <div class="dashboard-kpis" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1.5rem; margin-bottom: 2rem;">
                    <div class="kpi-card" onclick="navigateToSection('pending-applications')" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); cursor: pointer; transition: all 0.3s; border: 2px solid transparent;" onmouseover="this.style.transform='translateY(-5px)'; this.style.boxShadow='0 4px 8px rgba(0,0,0,0.15)'; this.style.borderColor='var(--primary-color)';" onmouseout="this.style.transform=''; this.style.boxShadow='0 2px 4px rgba(0,0,0,0.1)'; this.style.borderColor='transparent';">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Total Policies</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--primary-color);">${totalPolicies}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.5rem;">Click to view details →</div>
                    </div>
                    <div class="kpi-card" onclick="navigateToSection('approved-applications')" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); cursor: pointer; transition: all 0.3s; border: 2px solid transparent;" onmouseover="this.style.transform='translateY(-5px)'; this.style.boxShadow='0 4px 8px rgba(0,0,0,0.15)'; this.style.borderColor='var(--success-color)';" onmouseout="this.style.transform=''; this.style.boxShadow='0 2px 4px rgba(0,0,0,0.1)'; this.style.borderColor='transparent';">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Active Policies</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--success-color);">${activePolicies}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.5rem;">Click to view details →</div>
                    </div>
                    <div class="kpi-card" onclick="navigateToSection('pending-applications')" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); cursor: pointer; transition: all 0.3s; border: 2px solid transparent;" onmouseover="this.style.transform='translateY(-5px)'; this.style.boxShadow='0 4px 8px rgba(0,0,0,0.15)'; this.style.borderColor='var(--warning-color)';" onmouseout="this.style.transform=''; this.style.boxShadow='0 2px 4px rgba(0,0,0,0.1)'; this.style.borderColor='transparent';">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Pending Approval</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--warning-color);">${pendingPolicies}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.5rem;">Click to review →</div>
                    </div>
                    <div class="kpi-card" onclick="navigateToSection('rejected-applications')" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); cursor: pointer; transition: all 0.3s; border: 2px solid transparent;" onmouseover="this.style.transform='translateY(-5px)'; this.style.boxShadow='0 4px 8px rgba(0,0,0,0.15)'; this.style.borderColor='var(--danger-color)';" onmouseout="this.style.transform=''; this.style.boxShadow='0 2px 4px rgba(0,0,0,0.1)'; this.style.borderColor='transparent';">
                        <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Rejected</div>
                        <div style="font-size: 2rem; font-weight: 700; color: var(--danger-color);">${rejectedPolicies}</div>
                        <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.5rem;">Click to view details →</div>
                    </div>
                </div>
                
                <!-- Charts Row -->
                <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; margin-bottom: 2rem;">
                    <!-- Business Overview by Region -->
                    <div class="chart-card" style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Business Overview by Region</h3>
                        <div style="height: 300px; display: flex; align-items: flex-end; gap: 0.5rem; border-bottom: 2px solid var(--border-color); padding-bottom: 2.5rem;">
                            ${regionData.slice(0, 8).map(region => {
                                const maxPolicies = Math.max(...regionData.map(r => r.policies));
                                const height = maxPolicies > 0 ? (region.policies / maxPolicies) * 100 : 20;
                                return `
                                    <div style="flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: flex-end;">
                                        <div style="width: 100%; background: linear-gradient(180deg, var(--primary-color) 0%, var(--primary-hover) 100%); height: ${height}%; min-height: 20px; border-radius: 4px 4px 0 0; margin-bottom: 0.5rem;"></div>
                                        <div style="font-size: 0.7rem; color: var(--text-muted); text-align: center; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; width: 100%; padding: 0 0.25rem;">${region.name}</div>
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
        
        // Load policies table after rendering
        setTimeout(() => {
            loadAllPoliciesTable();
        }, 100);
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

// Refresh Dashboard
async function refreshDashboard() {
    const btn = document.getElementById('refreshDashboardBtn');
    const icon = document.getElementById('refreshIcon');
    if (btn) {
        btn.disabled = true;
        btn.style.opacity = '0.6';
        if (icon) icon.style.animation = 'spin 1s linear infinite';
    }
    
    try {
        await loadDashboard();
        // Show success feedback
        if (btn) {
            btn.style.background = 'var(--success-color)';
            setTimeout(() => {
                btn.style.background = 'var(--primary-color)';
                btn.disabled = false;
                btn.style.opacity = '1';
                if (icon) icon.style.animation = '';
            }, 1000);
        }
    } catch (error) {
        console.error('Error refreshing dashboard:', error);
        if (btn) {
            btn.disabled = false;
            btn.style.opacity = '1';
            if (icon) icon.style.animation = '';
        }
    }
}

// Navigate to section
function navigateToSection(sectionId) {
    const navBtn = document.querySelector(`[data-section="${sectionId}"]`);
    if (navBtn) {
        navBtn.click();
    }
}

// Load All Policies Table
let allPoliciesData = [];

async function loadAllPoliciesTable() {
    const tableContainer = document.getElementById('allPoliciesTable');
    if (!tableContainer) return;
    
    const btn = document.getElementById('refreshPoliciesBtn');
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<span style="animation: spin 1s linear infinite;">🔄</span> Loading...';
    }
    
    tableContainer.innerHTML = '<p class="loading-text">Loading policies...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        const region = document.getElementById('filterRegion')?.value || '';
        const broker = document.getElementById('filterBroker')?.value || '';
        const status = document.getElementById('filterStatus')?.value || '';
        const startDate = document.getElementById('filterStartDate')?.value || '';
        const endDate = document.getElementById('filterEndDate')?.value || '';
        
        let url = `${API_BASE_URL}/policy-approval/all`;
        const params = new URLSearchParams();
        if (region) params.append('region', region);
        if (broker) params.append('brokerId', broker);
        if (status) params.append('status', status);
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        
        if (params.toString()) {
            url += '?' + params.toString();
        }
        
        const response = await fetch(url, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (!response.ok) {
            throw new Error('Failed to load policies');
        }
        
        allPoliciesData = await response.json();
        renderPoliciesTable(allPoliciesData);
        
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<span>🔄</span> Refresh';
        }
    } catch (error) {
        console.error('Error loading policies:', error);
        tableContainer.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading policies</p>';
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<span>🔄</span> Refresh';
        }
    }
}

// Filter Policies
function filterPolicies() {
    if (allPoliciesData.length === 0) {
        loadAllPoliciesTable();
        return;
    }
    
    const region = document.getElementById('filterRegion')?.value || '';
    const broker = document.getElementById('filterBroker')?.value || '';
    const status = document.getElementById('filterStatus')?.value || '';
    const startDate = document.getElementById('filterStartDate')?.value || '';
    const endDate = document.getElementById('filterEndDate')?.value || '';
    
    let filtered = [...allPoliciesData];
    
    if (region) {
        filtered = filtered.filter(p => 
            (p.policyHolderProvince && p.policyHolderProvince.toLowerCase().includes(region.toLowerCase())) ||
            (p.policyHolderCity && p.policyHolderCity.toLowerCase().includes(region.toLowerCase()))
        );
    }
    
    if (broker) {
        filtered = filtered.filter(p => p.brokerId === broker);
    }
    
    if (status) {
        filtered = filtered.filter(p => p.status === status);
    }
    
    if (startDate) {
        const start = new Date(startDate);
        filtered = filtered.filter(p => new Date(p.createdAt) >= start);
    }
    
    if (endDate) {
        const end = new Date(endDate);
        end.setHours(23, 59, 59, 999);
        filtered = filtered.filter(p => new Date(p.createdAt) <= end);
    }
    
    renderPoliciesTable(filtered);
}

// Render Policies Table
function renderPoliciesTable(policies) {
    const tableContainer = document.getElementById('allPoliciesTable');
    if (!tableContainer) return;
    
    if (policies.length === 0) {
        tableContainer.innerHTML = '<p style="color: var(--text-secondary); padding: 2rem; text-align: center;">No policies found matching the filters.</p>';
        return;
    }
    
    tableContainer.innerHTML = `
        <div style="overflow-x: auto;">
            <table style="width: 100%; border-collapse: collapse;">
                <thead>
                    <tr style="background: var(--bg-secondary); border-bottom: 2px solid var(--border-color);">
                        <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: var(--text-primary);">Policy #</th>
                        <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: var(--text-primary);">Policy Holder</th>
                        <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: var(--text-primary);">Broker</th>
                        <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: var(--text-primary);">Service Type</th>
                        <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: var(--text-primary);">Status</th>
                        <th style="padding: 0.75rem; text-align: right; font-weight: 600; color: var(--text-primary);">Coverage</th>
                        <th style="padding: 0.75rem; text-align: right; font-weight: 600; color: var(--text-primary);">Premium</th>
                        <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: var(--text-primary);">Created</th>
                    </tr>
                </thead>
                <tbody>
                    ${policies.map(policy => `
                        <tr style="border-bottom: 1px solid var(--border-color); transition: background 0.2s;" 
                            onmouseover="this.style.background='var(--bg-secondary)';" 
                            onmouseout="this.style.background='';"
                            onclick="viewPolicyDetails('${policy.id}')" 
                            style="cursor: pointer;">
                            <td style="padding: 0.75rem; color: var(--text-primary); font-weight: 500;">${policy.policyNumber}</td>
                            <td style="padding: 0.75rem; color: var(--text-primary);">${policy.policyHolderName}</td>
                            <td style="padding: 0.75rem; color: var(--text-primary);">${policy.brokerName}</td>
                            <td style="padding: 0.75rem; color: var(--text-secondary);">${policy.serviceType}</td>
                            <td style="padding: 0.75rem;">
                                <span style="padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; 
                                    background: ${getStatusColor(policy.status)}; 
                                    color: ${getStatusTextColor(policy.status)};">
                                    ${policy.status}
                                </span>
                            </td>
                            <td style="padding: 0.75rem; text-align: right; color: var(--text-primary);">R ${(policy.coverageAmount || 0).toFixed(2)}</td>
                            <td style="padding: 0.75rem; text-align: right; color: var(--text-primary);">R ${(policy.premiumAmount || 0).toFixed(2)}</td>
                            <td style="padding: 0.75rem; color: var(--text-muted); font-size: 0.875rem;">${new Date(policy.createdAt).toLocaleDateString()}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
}

// Get Status Color
function getStatusColor(status) {
    const colors = {
        'Draft': '#6c757d',
        'PendingSubmission': '#ffc107',
        'Submitted': '#17a2b8',
        'UnderReview': '#17a2b8',
        'Approved': '#28a745',
        'Active': '#28a745',
        'Rejected': '#dc3545',
        'Cancelled': '#6c757d',
        'ChangesRequired': '#fd7e14'
    };
    return colors[status] || '#6c757d';
}

// Get Status Text Color
function getStatusTextColor(status) {
    return ['PendingSubmission', 'ChangesRequired'].includes(status) ? '#000' : '#fff';
}

// View Policy Details
function viewPolicyDetails(policyId) {
    // Navigate to pending applications and highlight the policy
    navigateToSection('pending-applications');
    // TODO: Implement policy detail view
}

// Load Broker Activity Dashboard
async function loadBrokerActivity() {
    const content = document.getElementById('brokerActivityContent');
    if (!content) return;
    
    content.innerHTML = '<p class="loading-text">Loading broker activity...</p>';
    
    try {
        const token = localStorage.getItem('managerToken');
        
        // Fetch broker statistics
        const statsResponse = await fetch(`${API_BASE_URL}/policy-approval/brokers/activity/stats`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        // Fetch latest policies sold by brokers
        const policiesResponse = await fetch(`${API_BASE_URL}/policy-approval/brokers/activity/latest-policies`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        // Fetch broker performance data
        const performanceResponse = await fetch(`${API_BASE_URL}/policy-approval/brokers/activity/performance`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        const stats = statsResponse.ok ? await statsResponse.json() : { active: 0, expired: 0, pending: 0 };
        const latestPolicies = policiesResponse.ok ? await policiesResponse.json() : [];
        const performance = performanceResponse.ok ? await performanceResponse.json() : [];
        
        // Calculate percentages for donut charts
        const totalPolicies = stats.active + stats.expired + stats.pending;
        const activePercent = totalPolicies > 0 ? (stats.active / totalPolicies) * 100 : 0;
        const expiredPercent = totalPolicies > 0 ? (stats.expired / totalPolicies) * 100 : 0;
        const pendingPercent = totalPolicies > 0 ? (stats.pending / totalPolicies) * 100 : 0;
        
        // Calculate last 7 days stats
        const last7Days = performance.filter(p => {
            const date = new Date(p.date);
            const sevenDaysAgo = new Date();
            sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
            return date >= sevenDaysAgo;
        });
        const last7DaysPending = last7Days.reduce((sum, p) => sum + (p.pending || 0), 0);
        
        content.innerHTML = `
            <div style="background: #f0f8f4; padding: 1.5rem; border-radius: 8px; min-height: 100vh;">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
                    <h2 style="margin: 0; color: var(--text-primary);">Broker Activity Dashboard</h2>
                    <button onclick="refreshBrokerActivity()" id="refreshBrokerActivityBtn" style="padding: 0.5rem 1rem; background: var(--primary-color); color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 0.875rem; display: flex; align-items: center; gap: 0.5rem; transition: all 0.3s;">
                        <span id="refreshBrokerIcon">🔄</span> Refresh
                    </button>
                </div>
                
                <!-- Main Stats Grid -->
                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 2rem;">
                    <!-- Status Section -->
                    <div style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Status</h3>
                        <div style="display: flex; align-items: center; gap: 2rem;">
                            <div style="flex: 1;">
                                <div style="margin-bottom: 1rem;">
                                    <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem;">
                                        <div style="width: 12px; height: 12px; background: var(--primary-color); border-radius: 2px;"></div>
                                        <span style="font-size: 0.875rem; color: var(--text-secondary);">Active ${stats.active}</span>
                                    </div>
                                    <div style="width: 100%; height: 8px; background: var(--bg-secondary); border-radius: 4px; overflow: hidden;">
                                        <div style="width: ${activePercent}%; height: 100%; background: var(--primary-color); transition: width 0.3s;"></div>
                                    </div>
                                </div>
                                <div>
                                    <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem;">
                                        <div style="width: 12px; height: 12px; background: #1a4d2e; border-radius: 2px;"></div>
                                        <span style="font-size: 0.875rem; color: var(--text-secondary);">Expired ${stats.expired}</span>
                                    </div>
                                    <div style="width: 100%; height: 8px; background: var(--bg-secondary); border-radius: 4px; overflow: hidden;">
                                        <div style="width: ${expiredPercent}%; height: 100%; background: #1a4d2e; transition: width 0.3s;"></div>
                                    </div>
                                </div>
                            </div>
                            <div style="position: relative; width: 150px; height: 150px;">
                                <svg viewBox="0 0 100 100" style="transform: rotate(-90deg); width: 100%; height: 100%;">
                                    <circle cx="50" cy="50" r="40" fill="none" stroke="var(--bg-secondary)" stroke-width="8"></circle>
                                    <circle cx="50" cy="50" r="40" fill="none" stroke="var(--primary-color)" stroke-width="8" 
                                        stroke-dasharray="${2 * Math.PI * 40}" 
                                        stroke-dashoffset="${2 * Math.PI * 40 * (1 - activePercent / 100)}"
                                        style="transition: stroke-dashoffset 0.3s;"></circle>
                                    <circle cx="50" cy="50" r="40" fill="none" stroke="#1a4d2e" stroke-width="8" 
                                        stroke-dasharray="${2 * Math.PI * 40}" 
                                        stroke-dashoffset="${2 * Math.PI * 40 * (1 - (activePercent + expiredPercent) / 100)}"
                                        style="transition: stroke-dashoffset 0.3s;"></circle>
                                </svg>
                                <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); text-align: center;">
                                    <div style="font-size: 1.5rem; font-weight: 700; color: var(--text-primary);">${totalPolicies}</div>
                                    <div style="font-size: 0.75rem; color: var(--text-muted);">Total</div>
                                </div>
                            </div>
                            <div style="flex: 1;">
                                <div style="margin-top: 1rem; padding-top: 1rem; border-top: 1px solid var(--border-color);">
                                    <div style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 0.5rem;">Last 7 Days</div>
                                    <div style="display: flex; align-items: center; gap: 0.5rem;">
                                        <div style="width: 12px; height: 12px; background: var(--success-color); border-radius: 2px;"></div>
                                        <span style="font-size: 0.875rem; color: var(--text-secondary);">Pending ${last7DaysPending}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Pending Activities Section -->
                    <div style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Pending Activities</h3>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem;">
                            <!-- Policy Review -->
                            <div>
                                <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem;">
                                    <div>
                                        <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem;">
                                            <div style="width: 8px; height: 8px; background: #000; border-radius: 50%;"></div>
                                            <span style="font-size: 0.75rem; color: var(--text-secondary);">Reviewed ${stats.reviewed || 0}</span>
                                        </div>
                                        <div style="display: flex; align-items: center; gap: 0.5rem;">
                                            <div style="width: 8px; height: 8px; background: var(--primary-color); border-radius: 50%;"></div>
                                            <span style="font-size: 0.75rem; color: var(--text-secondary);">Pending ${stats.pendingReview || 0}</span>
                                        </div>
                                    </div>
                                    <div style="position: relative; width: 80px; height: 80px;">
                                        <svg viewBox="0 0 100 100" style="transform: rotate(-90deg); width: 100%; height: 100%;">
                                            <circle cx="50" cy="50" r="35" fill="none" stroke="var(--bg-secondary)" stroke-width="6"></circle>
                                            <circle cx="50" cy="50" r="35" fill="none" stroke="var(--primary-color)" stroke-width="6" 
                                                stroke-dasharray="${2 * Math.PI * 35}" 
                                                stroke-dashoffset="${2 * Math.PI * 35 * (1 - ((stats.pendingReview || 0) / ((stats.reviewed || 0) + (stats.pendingReview || 0) || 1)))}"></circle>
                                        </svg>
                                    </div>
                                </div>
                                <div style="font-size: 0.875rem; color: var(--text-primary); font-weight: 500; text-align: center;">Policy Review</div>
                            </div>
                            
                            <!-- Policy Renewals -->
                            <div>
                                <div style="margin-bottom: 1rem; padding-top: 1rem; border-top: 1px solid var(--border-color);">
                                    <div style="font-size: 0.75rem; color: var(--text-muted); margin-bottom: 0.5rem;">Last 7 Days</div>
                                    <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem;">
                                        <div style="width: 8px; height: 8px; background: #000; border-radius: 50%;"></div>
                                        <span style="font-size: 0.75rem; color: var(--text-secondary);">Reviewed ${stats.renewalsReviewed || 0}</span>
                                    </div>
                                    <div style="display: flex; align-items: center; gap: 0.5rem;">
                                        <div style="width: 8px; height: 8px; background: var(--primary-color); border-radius: 50%;"></div>
                                        <span style="font-size: 0.75rem; color: var(--text-secondary);">Pending ${stats.renewalsPending || 0}</span>
                                    </div>
                                </div>
                                <div style="position: relative; width: 80px; height: 80px; margin: 0 auto;">
                                    <svg viewBox="0 0 100 100" style="transform: rotate(-90deg); width: 100%; height: 100%;">
                                        <circle cx="50" cy="50" r="35" fill="none" stroke="var(--bg-secondary)" stroke-width="6"></circle>
                                        <circle cx="50" cy="50" r="35" fill="none" stroke="var(--primary-color)" stroke-width="6" 
                                            stroke-dasharray="${2 * Math.PI * 35}" 
                                            stroke-dashoffset="${2 * Math.PI * 35 * (1 - ((stats.renewalsPending || 0) / ((stats.renewalsReviewed || 0) + (stats.renewalsPending || 0) || 1)))}"></circle>
                                    </svg>
                                </div>
                                <div style="font-size: 0.875rem; color: var(--text-primary); font-weight: 500; text-align: center; margin-top: 0.5rem;">Policy Renewals</div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Latest Policies Sold Section -->
                <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem;">
                    <!-- Latest Policies Sold by Brokers -->
                    <div style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Latest Policies Sold</h3>
                        <div style="max-height: 500px; overflow-y: auto;">
                            ${latestPolicies.length > 0 ? latestPolicies.map(policy => `
                                <div style="display: flex; align-items: center; gap: 1rem; padding: 1rem; border-bottom: 1px solid var(--border-color); transition: background 0.2s;" 
                                    onmouseover="this.style.background='var(--bg-secondary)';" 
                                    onmouseout="this.style.background='';">
                                    <div style="width: 40px; height: 40px; border-radius: 50%; background: var(--primary-color); display: flex; align-items: center; justify-content: center; color: white; font-weight: 600; font-size: 0.875rem;">
                                        ${policy.brokerName ? policy.brokerName.charAt(0).toUpperCase() : 'B'}
                                    </div>
                                    <div style="flex: 1;">
                                        <div style="font-weight: 600; color: var(--text-primary); margin-bottom: 0.25rem;">${policy.brokerName || 'Unknown Broker'}</div>
                                        <div style="font-size: 0.875rem; color: var(--text-secondary); margin-bottom: 0.25rem;">${policy.brokerPhone || 'N/A'}</div>
                                        <div style="font-size: 0.875rem; color: var(--text-muted);">
                                            ${policy.serviceType || 'N/A'} (${policy.startDate ? new Date(policy.startDate).toLocaleDateString() : 'N/A'} to ${policy.endDate ? new Date(policy.endDate).toLocaleDateString() : 'N/A'})
                                        </div>
                                    </div>
                                    <div>
                                        <span style="padding: 0.375rem 0.75rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; 
                                            background: ${getStatusColor(policy.status)}; 
                                            color: ${getStatusTextColor(policy.status)};">
                                            ${policy.status || 'Draft'}
                                        </span>
                                    </div>
                                </div>
                            `).join('') : '<p style="color: var(--text-secondary); padding: 2rem; text-align: center;">No policies found.</p>'}
                        </div>
                    </div>
                    
                    <!-- Broker Performance Summary -->
                    <div style="background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <h3 style="margin: 0 0 1.5rem 0; color: var(--text-primary); font-size: 1.25rem;">Top Performers</h3>
                        <div style="display: flex; flex-direction: column; gap: 1rem;">
                            ${performance.slice(0, 5).map((broker, index) => `
                                <div style="display: flex; align-items: center; gap: 1rem; padding: 0.75rem; background: var(--bg-secondary); border-radius: 4px;">
                                    <div style="width: 30px; height: 30px; border-radius: 50%; background: var(--primary-color); display: flex; align-items: center; justify-content: center; color: white; font-weight: 600; font-size: 0.75rem;">
                                        ${index + 1}
                                    </div>
                                    <div style="flex: 1;">
                                        <div style="font-weight: 600; color: var(--text-primary); font-size: 0.875rem;">${broker.brokerName || 'Unknown'}</div>
                                        <div style="font-size: 0.75rem; color: var(--text-muted);">${broker.policiesCount || 0} policies</div>
                                    </div>
                                    <div style="font-weight: 600; color: var(--primary-color);">R ${(broker.totalPremium || 0).toFixed(2)}</div>
                                </div>
                            `).join('')}
                            ${performance.length === 0 ? '<p style="color: var(--text-secondary); text-align: center; padding: 1rem;">No performance data available.</p>' : ''}
                        </div>
                    </div>
                </div>
            </div>
        `;
    } catch (error) {
        console.error('Error loading broker activity:', error);
        content.innerHTML = '<p class="loading-text" style="color: var(--danger-color);">Error loading broker activity</p>';
    }
}

// Refresh Broker Activity
async function refreshBrokerActivity() {
    const btn = document.getElementById('refreshBrokerActivityBtn');
    const icon = document.getElementById('refreshBrokerIcon');
    if (btn) {
        btn.disabled = true;
        btn.style.opacity = '0.6';
        if (icon) icon.style.animation = 'spin 1s linear infinite';
    }
    
    try {
        await loadBrokerActivity();
        if (btn) {
            btn.style.background = 'var(--success-color)';
            setTimeout(() => {
                btn.style.background = 'var(--primary-color)';
                btn.disabled = false;
                btn.style.opacity = '1';
                if (icon) icon.style.animation = '';
            }, 1000);
        }
    } catch (error) {
        console.error('Error refreshing broker activity:', error);
        if (btn) {
            btn.disabled = false;
            btn.style.opacity = '1';
            if (icon) icon.style.animation = '';
        }
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
    loadSectionData('dashboard');
});

