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
        'create-client': 'New Application',
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
                <h3 style="margin-bottom: 2rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Personal Information</h3>
                <div class="form-grid">
                    <div class="form-group">
                        <label for="lastName" class="required">Last Name (Apelyido)</label>
                        <input type="text" id="lastName" name="lastName" required>
                    </div>
                    <div class="form-group">
                        <label for="firstName" class="required">First Name (Pangalan)</label>
                        <input type="text" id="firstName" name="firstName" required>
                    </div>
                    <div class="form-group">
                        <label for="middleName">Middle Name (Gitnang Pangalan)</label>
                        <input type="text" id="middleName" name="middleName">
                    </div>
                    <div class="form-group">
                        <label for="idNumber" class="required">ID Number</label>
                        <input type="text" id="idNumber" name="idNumber" required maxlength="13">
                        <small>South African ID number (13 digits)</small>
                    </div>
                    <div class="form-group">
                        <label for="dateOfBirth" class="required">Date of Birth (Petsa ng Kapanganakan)</label>
                        <input type="date" id="dateOfBirth" name="dateOfBirth" required>
                    </div>
                    <div class="form-group">
                        <label for="birthplace">Birthplace (Lugar ng Kapanganakan)</label>
                        <input type="text" id="birthplace" name="birthplace">
                    </div>
                    <div class="form-group">
                        <label for="sex" class="required">Sex (Kasarian)</label>
                        <select id="sex" name="sex" required>
                            <option value="">-- Select --</option>
                            <option value="Male">Male</option>
                            <option value="Female">Female</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label for="civilStatus" class="required">Civil Status (Katayuang Sibil)</label>
                        <select id="civilStatus" name="civilStatus" required>
                            <option value="">-- Select --</option>
                            <option value="Single">Single</option>
                            <option value="Married">Married</option>
                            <option value="Divorced">Divorced</option>
                            <option value="Widowed">Widowed</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label for="occupation">Occupation (Trabaho)</label>
                        <input type="text" id="occupation" name="occupation">
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
                        <label for="address" class="required">Residence Address (Tirahan)</label>
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
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Beneficiaries (Mga Kaanak na Tatanggap ng Benepisyo)</h3>
                <div class="form-grid">
                    <div class="form-group full-width">
                        <div id="beneficiariesTable" style="overflow-x: auto;">
                            <table style="width: 100%; border-collapse: collapse; background-color: var(--bg-secondary); border-radius: 6px;">
                                <thead>
                                    <tr style="border-bottom: 1px solid var(--border-color);">
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Full Name</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Date of Birth</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Age</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Mobile No.</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Email</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Relationship</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Type</th>
                                        <th style="padding: 0.75rem; text-align: left; color: var(--text-primary); font-weight: 500;">Action</th>
                                    </tr>
                                </thead>
                                <tbody id="beneficiariesTableBody">
                                    <!-- Beneficiaries will be added here -->
                                </tbody>
                            </table>
                        </div>
                        <button type="button" id="addBeneficiaryBtn" class="btn btn-secondary" style="margin-top: 1rem;">+ Add Beneficiary</button>
                        <small style="display: block; margin-top: 0.5rem; color: var(--text-muted);">Add beneficiaries who will receive benefits</small>
                    </div>
                </div>
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Declarations and Agreements</h3>
                <div class="form-grid">
                    <div class="form-group full-width">
                        <div style="background-color: var(--bg-secondary); padding: 1.5rem; border-radius: 6px; border: 1px solid var(--border-color);">
                            <p style="color: var(--text-secondary); line-height: 1.6; margin-bottom: 1rem;">
                                I hereby apply for insurance coverage for which I am or may have become eligible, subject to the terms and conditions of the policy. I hereby declare that all statements and answers contained in this application form are true and complete. I understand that the insurance applied for will not become effective until the payment of the premium and until this application is approved.
                            </p>
                            <p style="color: var(--text-secondary); line-height: 1.6; margin-bottom: 1rem;">
                                I hereby agree to the recording of all telephone calls with the insurance company and authorize them to share information for purposes relating to the insurance coverage, training and quality assurance.
                            </p>
                            <div class="form-group" style="margin-top: 1rem;">
                                <label style="display: flex; align-items: center; cursor: pointer;">
                                    <input type="checkbox" id="declarationsAgree" name="declarationsAgree" required style="margin-right: 0.5rem; width: auto;">
                                    <span style="color: var(--text-primary);">I have read and agree to the declarations and agreements above</span>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Data Privacy Consent</h3>
                <div class="form-grid">
                    <div class="form-group full-width">
                        <div style="background-color: var(--bg-secondary); padding: 1.5rem; border-radius: 6px; border: 1px solid var(--border-color);">
                            <p style="color: var(--text-secondary); line-height: 1.6; margin-bottom: 1rem;">
                                I agree and consent that the insurance company may collect, use, and process my personal information contained in this insurance application form for the following purposes:
                            </p>
                            <ul style="color: var(--text-secondary); line-height: 1.8; margin-left: 1.5rem; margin-bottom: 1rem;">
                                <li>To process my application</li>
                                <li>To administer my policy/policies</li>
                                <li>To provide customer service and support</li>
                                <li>To research and conduct data analytics to improve customer service</li>
                                <li>To inform me of latest updates, special offers, and event invites related to my policy/policies</li>
                            </ul>
                            <p style="color: var(--text-secondary); line-height: 1.6; margin-bottom: 1rem; font-size: 0.875rem;">
                                I am aware and have read the Data Privacy Notice, which contains my rights as a data subject, including the right to access and correction, and the right to object.
                            </p>
                            <div class="form-group" style="margin-top: 1rem;">
                                <label style="display: flex; align-items: center; cursor: pointer;">
                                    <input type="checkbox" id="dataPrivacyConsent" name="dataPrivacyConsent" required style="margin-right: 0.5rem; width: auto;">
                                    <span style="color: var(--text-primary);">I consent to the collection and processing of my personal data as described above</span>
                                </label>
                            </div>
                            <div class="form-group" style="margin-top: 1rem;">
                                <label style="display: flex; align-items: center; cursor: pointer;">
                                    <input type="checkbox" id="marketingConsent" name="marketingConsent" style="margin-right: 0.5rem; width: auto;">
                                    <span style="color: var(--text-primary); font-size: 0.9375rem;">Yes, I would like to receive special offers, event invitations, and updates from the insurance company and its partners</span>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                
                <h3 style="margin: 3rem 0 1.5rem; color: var(--text-primary); font-weight: 600; font-size: 1.5rem;">Agency/Payor Information</h3>
                <div class="form-grid">
                    <div class="form-group">
                        <label for="employmentStartDate">Term of Employment - From / Departure Date</label>
                        <input type="date" id="employmentStartDate" name="employmentStartDate">
                    </div>
                    <div class="form-group">
                        <label for="employmentEndDate">Term of Employment - To / End of Employment Contract</label>
                        <input type="date" id="employmentEndDate" name="employmentEndDate">
                    </div>
                    <div class="form-group">
                        <label for="agencyName">Name of Agency</label>
                        <input type="text" id="agencyName" name="agencyName">
                    </div>
                    <div class="form-group">
                        <label for="agencyContactNo">Agency Contact No.</label>
                        <input type="tel" id="agencyContactNo" name="agencyContactNo">
                    </div>
                    <div class="form-group full-width">
                        <label for="agencyAddress">Address of Agency</label>
                        <textarea id="agencyAddress" name="agencyAddress" rows="2"></textarea>
                    </div>
                    <div class="form-group">
                        <label for="agencyEmail">E-mail Address of Agency</label>
                        <input type="email" id="agencyEmail" name="agencyEmail">
                    </div>
                    <div class="form-group">
                        <label for="agencySignatory">Signature over Printed Name of Authorized Signatory</label>
                        <input type="text" id="agencySignatory" name="agencySignatory">
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
                    <div style="display: flex; gap: 1rem; flex-wrap: wrap;">
                        <button type="button" class="btn btn-secondary" id="cancelBtn">Cancel</button>
                        <button type="button" class="btn btn-secondary" id="downloadPdfBtn">Download PDF</button>
                        <button type="button" class="btn btn-secondary" id="emailPdfBtn">Email PDF</button>
                        <button type="submit" class="btn btn-primary" id="submitBtn">Create Client & Policy</button>
                    </div>
                </div>
            </form>
        `;
        
        // Add PDF download handler
        const downloadPdfBtn = document.getElementById('downloadPdfBtn');
        if (downloadPdfBtn) {
            downloadPdfBtn.addEventListener('click', generateAndDownloadPDF);
        }
        
        // Add email PDF handler
        const emailPdfBtn = document.getElementById('emailPdfBtn');
        if (emailPdfBtn) {
            emailPdfBtn.addEventListener('click', showEmailModal);
        }
        
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
                document.getElementById('beneficiariesTableBody').innerHTML = '';
                beneficiaryCounter = 0;
                const errorDiv = document.getElementById('formError');
                const successDiv = document.getElementById('formSuccess');
                if (errorDiv) errorDiv.classList.remove('show');
                if (successDiv) successDiv.classList.remove('show');
            });
        }
        
        // Add beneficiary functionality
        let beneficiaryCounter = 0;
        const addBeneficiaryBtn = document.getElementById('addBeneficiaryBtn');
        const beneficiariesTableBody = document.getElementById('beneficiariesTableBody');
        
        if (addBeneficiaryBtn && beneficiariesTableBody) {
            addBeneficiaryBtn.addEventListener('click', () => {
                const row = document.createElement('tr');
                row.id = `beneficiary-row-${beneficiaryCounter}`;
                row.innerHTML = `
                    <td style="padding: 0.5rem;">
                        <input type="text" name="beneficiary-fullName-${beneficiaryCounter}" required style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                    </td>
                    <td style="padding: 0.5rem;">
                        <input type="date" name="beneficiary-dob-${beneficiaryCounter}" style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                    </td>
                    <td style="padding: 0.5rem;">
                        <input type="number" name="beneficiary-age-${beneficiaryCounter}" min="0" style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                    </td>
                    <td style="padding: 0.5rem;">
                        <input type="tel" name="beneficiary-mobile-${beneficiaryCounter}" style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                    </td>
                    <td style="padding: 0.5rem;">
                        <input type="email" name="beneficiary-email-${beneficiaryCounter}" style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                    </td>
                    <td style="padding: 0.5rem;">
                        <input type="text" name="beneficiary-relationship-${beneficiaryCounter}" placeholder="e.g., Spouse, Child" style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                    </td>
                    <td style="padding: 0.5rem;">
                        <select name="beneficiary-type-${beneficiaryCounter}" required style="width: 100%; padding: 0.5rem; background-color: var(--bg-tertiary); border: 1px solid var(--border-color); border-radius: 4px; color: var(--text-primary);">
                            <option value="">-- Select --</option>
                            <option value="Revocable">Revocable</option>
                            <option value="Irrevocable">Irrevocable</option>
                        </select>
                    </td>
                    <td style="padding: 0.5rem;">
                        <button type="button" class="btn btn-secondary" onclick="removeBeneficiary(${beneficiaryCounter})" style="padding: 0.375rem 0.75rem; font-size: 0.875rem;">Remove</button>
                    </td>
                `;
                beneficiariesTableBody.appendChild(row);
                beneficiaryCounter++;
            });
        }
        
        // Global function to remove beneficiary
        window.removeBeneficiary = function(index) {
            const row = document.getElementById(`beneficiary-row-${index}`);
            if (row) {
                row.remove();
            }
        };
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
    
    // Collect beneficiaries
    const beneficiaries = [];
    const beneficiaryRows = document.querySelectorAll('[id^="beneficiary-row-"]');
    beneficiaryRows.forEach(row => {
        const index = row.id.split('-')[2];
        const fullName = document.querySelector(`[name="beneficiary-fullName-${index}"]`)?.value.trim();
        if (fullName) {
            beneficiaries.push({
                fullName: fullName,
                dateOfBirth: document.querySelector(`[name="beneficiary-dob-${index}"]`)?.value || null,
                age: document.querySelector(`[name="beneficiary-age-${index}"]`)?.value ? parseInt(document.querySelector(`[name="beneficiary-age-${index}"]`).value) : null,
                mobile: document.querySelector(`[name="beneficiary-mobile-${index}"]`)?.value.trim() || null,
                email: document.querySelector(`[name="beneficiary-email-${index}"]`)?.value.trim() || null,
                relationship: document.querySelector(`[name="beneficiary-relationship-${index}"]`)?.value.trim() || null,
                type: document.querySelector(`[name="beneficiary-type-${index}"]`)?.value || null
            });
        }
    });
    
    // Get form values
    const formData = {
        firstName: document.getElementById('firstName').value.trim(),
        lastName: document.getElementById('lastName').value.trim(),
        middleName: document.getElementById('middleName')?.value.trim() || null,
        email: document.getElementById('email').value.trim(),
        phone: document.getElementById('phone').value.trim(),
        idNumber: document.getElementById('idNumber').value.trim(),
        dateOfBirth: document.getElementById('dateOfBirth').value,
        birthplace: document.getElementById('birthplace')?.value.trim() || null,
        sex: document.getElementById('sex').value,
        civilStatus: document.getElementById('civilStatus').value,
        occupation: document.getElementById('occupation')?.value.trim() || null,
        address: document.getElementById('address').value.trim(),
        serviceTypeId: document.getElementById('serviceType').value || null,
        coverageAmount: document.getElementById('coverageAmount').value ? parseFloat(document.getElementById('coverageAmount').value) : null,
        premiumAmount: document.getElementById('premiumAmount').value ? parseFloat(document.getElementById('premiumAmount').value) : null,
        activationDate: document.getElementById('activationDate').value || null,
        monthlyIncome: document.getElementById('monthlyIncome').value ? parseFloat(document.getElementById('monthlyIncome').value) : null,
        monthlyExpenses: document.getElementById('monthlyExpenses').value ? parseFloat(document.getElementById('monthlyExpenses').value) : null,
        paymentDate: document.getElementById('paymentDate').value ? parseInt(document.getElementById('paymentDate').value) : null,
        beneficiaries: beneficiaries,
        declarationsAgree: document.getElementById('declarationsAgree').checked,
        dataPrivacyConsent: document.getElementById('dataPrivacyConsent').checked,
        marketingConsent: document.getElementById('marketingConsent')?.checked || false,
        employmentStartDate: document.getElementById('employmentStartDate')?.value || null,
        employmentEndDate: document.getElementById('employmentEndDate')?.value || null,
        agencyName: document.getElementById('agencyName')?.value.trim() || null,
        agencyContactNo: document.getElementById('agencyContactNo')?.value.trim() || null,
        agencyAddress: document.getElementById('agencyAddress')?.value.trim() || null,
        agencyEmail: document.getElementById('agencyEmail')?.value.trim() || null,
        agencySignatory: document.getElementById('agencySignatory')?.value.trim() || null,
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
        if (formData.middleName) formDataToSend.append('middleName', formData.middleName);
        if (formData.birthplace) formDataToSend.append('birthplace', formData.birthplace);
        if (formData.sex) formDataToSend.append('sex', formData.sex);
        if (formData.civilStatus) formDataToSend.append('civilStatus', formData.civilStatus);
        if (formData.occupation) formDataToSend.append('occupation', formData.occupation);
        formDataToSend.append('beneficiaries', JSON.stringify(formData.beneficiaries));
        formDataToSend.append('declarationsAgree', formData.declarationsAgree.toString());
        formDataToSend.append('dataPrivacyConsent', formData.dataPrivacyConsent.toString());
        formDataToSend.append('marketingConsent', formData.marketingConsent.toString());
        if (formData.employmentStartDate) formDataToSend.append('employmentStartDate', formData.employmentStartDate);
        if (formData.employmentEndDate) formDataToSend.append('employmentEndDate', formData.employmentEndDate);
        if (formData.agencyName) formDataToSend.append('agencyName', formData.agencyName);
        if (formData.agencyContactNo) formDataToSend.append('agencyContactNo', formData.agencyContactNo);
        if (formData.agencyAddress) formDataToSend.append('agencyAddress', formData.agencyAddress);
        if (formData.agencyEmail) formDataToSend.append('agencyEmail', formData.agencyEmail);
        if (formData.agencySignatory) formDataToSend.append('agencySignatory', formData.agencySignatory);
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
