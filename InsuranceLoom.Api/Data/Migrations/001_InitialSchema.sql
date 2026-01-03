-- Insurance Loom Database Schema
-- PostgreSQL Migration Script

-- ============================================
-- CORE TABLES
-- ============================================

-- Users table (base for all user types)
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    user_type VARCHAR(20) NOT NULL CHECK (user_type IN ('Broker', 'PolicyHolder', 'Manager', 'Admin')),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Service Types
CREATE TABLE IF NOT EXISTS service_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    service_code VARCHAR(50) UNIQUE NOT NULL,
    service_name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- USER TYPE TABLES
-- ============================================

CREATE TABLE IF NOT EXISTS brokers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    agent_number VARCHAR(50) UNIQUE NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    company_name VARCHAR(200),
    phone VARCHAR(20),
    license_number VARCHAR(100),
    commission_rate DECIMAL(5,2),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS policy_holders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    policy_number VARCHAR(50) UNIQUE NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    id_number VARCHAR(20),
    phone VARCHAR(20),
    address TEXT,
    date_of_birth DATE,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS managers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    email VARCHAR(255) UNIQUE NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    employee_number VARCHAR(50) UNIQUE,
    department VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    can_approve_policies BOOLEAN DEFAULT true,
    can_manage_brokers BOOLEAN DEFAULT false,
    can_view_reports BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- POLICY TABLES
-- ============================================

CREATE TABLE IF NOT EXISTS policies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    policy_number VARCHAR(50) UNIQUE NOT NULL,
    policy_holder_id UUID REFERENCES policy_holders(id),
    broker_id UUID REFERENCES brokers(id),
    service_type_id UUID REFERENCES service_types(id),
    service_code VARCHAR(50),
    coverage_amount DECIMAL(15,2),
    premium_amount DECIMAL(10,2),
    start_date DATE NOT NULL,
    end_date DATE,
    status VARCHAR(20) DEFAULT 'Draft' CHECK (status IN ('Draft', 'PendingSubmission', 'Submitted', 'UnderReview', 'Approved', 'Active', 'Rejected', 'Cancelled', 'ChangesRequired')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- DOCUMENT MANAGEMENT TABLES
-- ============================================

CREATE TABLE IF NOT EXISTS document_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_code VARCHAR(50) UNIQUE NOT NULL,
    document_name VARCHAR(100) NOT NULL,
    description TEXT,
    service_type_id UUID REFERENCES service_types(id),
    is_required BOOLEAN DEFAULT false,
    is_optional BOOLEAN DEFAULT false,
    max_file_size_mb INT DEFAULT 10,
    allowed_file_types TEXT[],
    validity_period_days INT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_type_id UUID REFERENCES document_types(id),
    policy_holder_id UUID REFERENCES policy_holders(id),
    policy_id UUID REFERENCES policies(id),
    broker_id UUID REFERENCES brokers(id),
    file_name VARCHAR(255) NOT NULL,
    original_file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    file_type VARCHAR(50) NOT NULL,
    file_extension VARCHAR(10) NOT NULL,
    uploaded_by UUID REFERENCES users(id),
    upload_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expiry_date DATE,
    is_expired BOOLEAN DEFAULT false,
    status VARCHAR(20) DEFAULT 'Pending' CHECK (status IN ('Pending', 'Verified', 'Rejected', 'Expired')),
    verified_by UUID REFERENCES users(id),
    verified_date TIMESTAMP,
    rejection_reason TEXT,
    is_encrypted BOOLEAN DEFAULT true,
    checksum VARCHAR(64),
    metadata JSONB,
    tags TEXT[],
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS service_document_requirements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    service_type_id UUID REFERENCES service_types(id),
    document_type_id UUID REFERENCES document_types(id),
    is_required BOOLEAN DEFAULT false,
    is_conditional BOOLEAN DEFAULT false,
    condition_description TEXT,
    display_order INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- APPROVAL WORKFLOW TABLES
-- ============================================

CREATE TABLE IF NOT EXISTS policy_approvals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    policy_id UUID REFERENCES policies(id) ON DELETE CASCADE,
    broker_id UUID REFERENCES brokers(id),
    policy_holder_id UUID REFERENCES policy_holders(id),
    status VARCHAR(20) DEFAULT 'Pending' CHECK (status IN ('Pending', 'UnderReview', 'Approved', 'Rejected', 'RequiresChanges')),
    submitted_by UUID REFERENCES users(id),
    submitted_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    assigned_manager_id UUID REFERENCES managers(id),
    assigned_date TIMESTAMP,
    reviewed_by UUID REFERENCES managers(id),
    reviewed_date TIMESTAMP,
    review_notes TEXT,
    documents_verified BOOLEAN DEFAULT false,
    documents_verified_by UUID REFERENCES managers(id),
    documents_verified_date TIMESTAMP,
    approved_by UUID REFERENCES managers(id),
    approved_date TIMESTAMP,
    approval_notes TEXT,
    rejected_by UUID REFERENCES managers(id),
    rejected_date TIMESTAMP,
    rejection_reason TEXT,
    changes_requested_by UUID REFERENCES managers(id),
    changes_requested_date TIMESTAMP,
    changes_required TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS policy_approval_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    approval_id UUID REFERENCES policy_approvals(id),
    policy_id UUID REFERENCES policies(id),
    action VARCHAR(50) NOT NULL,
    performed_by UUID REFERENCES users(id),
    performed_by_type VARCHAR(20),
    notes TEXT,
    previous_status VARCHAR(20),
    new_status VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- OTHER TABLES
-- ============================================

CREATE TABLE IF NOT EXISTS claims (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    claim_number VARCHAR(50) UNIQUE NOT NULL,
    policy_id UUID REFERENCES policies(id),
    policy_holder_id UUID REFERENCES policy_holders(id),
    broker_id UUID REFERENCES brokers(id),
    claim_type VARCHAR(100),
    claim_amount DECIMAL(15,2),
    description TEXT,
    status VARCHAR(20) DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Processing')),
    submitted_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    policy_id UUID REFERENCES policies(id),
    policy_holder_id UUID REFERENCES policy_holders(id),
    payment_type VARCHAR(20) CHECK (payment_type IN ('Premium', 'Claim', 'Refund')),
    amount DECIMAL(10,2) NOT NULL,
    payment_method VARCHAR(50),
    payment_status VARCHAR(20) DEFAULT 'Pending' CHECK (payment_status IN ('Pending', 'Completed', 'Failed', 'Refunded')),
    transaction_reference VARCHAR(100),
    payment_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS debit_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    policy_id UUID REFERENCES policies(id),
    policy_holder_id UUID REFERENCES policy_holders(id),
    bank_account_number VARCHAR(50),
    bank_name VARCHAR(100),
    branch_code VARCHAR(20),
    amount DECIMAL(10,2) NOT NULL,
    frequency VARCHAR(20) CHECK (frequency IN ('Monthly', 'Quarterly', 'Annually')),
    next_debit_date DATE,
    status VARCHAR(20) DEFAULT 'Active' CHECK (status IN ('Active', 'Suspended', 'Cancelled')),
    last_debit_date DATE,
    retry_count INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS csv_imports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    broker_id UUID REFERENCES brokers(id),
    file_name VARCHAR(255) NOT NULL,
    total_records INT NOT NULL,
    successful_records INT DEFAULT 0,
    failed_records INT DEFAULT 0,
    status VARCHAR(20) DEFAULT 'Processing' CHECK (status IN ('Processing', 'Completed', 'Failed', 'Partial')),
    error_log JSONB,
    imported_by UUID REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS csv_import_errors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    import_id UUID REFERENCES csv_imports(id) ON DELETE CASCADE,
    row_number INT NOT NULL,
    error_message TEXT NOT NULL,
    row_data JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- INDEXES
-- ============================================

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_user_type ON users(user_type);
CREATE INDEX IF NOT EXISTS idx_brokers_agent_number ON brokers(agent_number);
CREATE INDEX IF NOT EXISTS idx_policy_holders_policy_number ON policy_holders(policy_number);
CREATE INDEX IF NOT EXISTS idx_managers_email ON managers(email);
CREATE INDEX IF NOT EXISTS idx_managers_employee_number ON managers(employee_number);
CREATE INDEX IF NOT EXISTS idx_policies_policy_number ON policies(policy_number);
CREATE INDEX IF NOT EXISTS idx_policies_status ON policies(status);
CREATE INDEX IF NOT EXISTS idx_policies_policy_holder_id ON policies(policy_holder_id);
CREATE INDEX IF NOT EXISTS idx_policies_broker_id ON policies(broker_id);
CREATE INDEX IF NOT EXISTS idx_documents_policy_id ON documents(policy_id);
CREATE INDEX IF NOT EXISTS idx_documents_policy_holder_id ON documents(policy_holder_id);
CREATE INDEX IF NOT EXISTS idx_documents_status ON documents(status);
CREATE INDEX IF NOT EXISTS idx_documents_expiry_date ON documents(expiry_date);
CREATE INDEX IF NOT EXISTS idx_policy_approvals_status ON policy_approvals(status);
CREATE INDEX IF NOT EXISTS idx_policy_approvals_assigned_manager ON policy_approvals(assigned_manager_id);
CREATE INDEX IF NOT EXISTS idx_policy_approvals_policy_id ON policy_approvals(policy_id);
CREATE INDEX IF NOT EXISTS idx_policy_approval_history_approval_id ON policy_approval_history(approval_id);

-- ============================================
-- SEED DATA
-- ============================================

-- Insert Service Types
INSERT INTO service_types (service_code, service_name, description) VALUES
('FUNERAL', 'Funeral Cover', 'Funeral insurance coverage'),
('LIFE', 'Life Insurance', 'Life insurance policy'),
('HEALTH', 'Health Insurance', 'Health and medical coverage'),
('DISABILITY', 'Disability Cover', 'Disability insurance'),
('INCOME', 'Income Protection', 'Income protection insurance')
ON CONFLICT (service_code) DO NOTHING;

-- Insert Document Types
INSERT INTO document_types (document_code, document_name, description, is_required, allowed_file_types, validity_period_days) VALUES
('ID_DOCUMENT', 'ID Document', 'South African ID or Passport', true, ARRAY['pdf', 'jpg', 'png', 'jpeg'], NULL),
('PROOF_OF_ADDRESS', 'Proof of Address', 'Utility bill, bank statement, or municipal account (not older than 3 months)', true, ARRAY['pdf', 'jpg', 'png', 'jpeg'], 90),
('PROOF_OF_INCOME', 'Proof of Income', 'Salary slip or bank statement', false, ARRAY['pdf', 'jpg', 'png', 'jpeg'], 90),
('MEDICAL_CERTIFICATE', 'Medical Certificate', 'Medical examination certificate', false, ARRAY['pdf', 'jpg', 'png', 'jpeg'], 365),
('BENEFICIARY_ID', 'Beneficiary ID Document', 'ID copy of beneficiary', false, ARRAY['pdf', 'jpg', 'png', 'jpeg'], NULL),
('BENEFICIARY_FORM', 'Beneficiary Nomination Form', 'Signed beneficiary nomination form', false, ARRAY['pdf'], NULL),
('HEALTH_DECLARATION', 'Health Declaration', 'Health and medical history declaration', false, ARRAY['pdf'], NULL),
('EMPLOYMENT_LETTER', 'Employment Letter', 'Letter from employer confirming employment', false, ARRAY['pdf', 'jpg', 'png', 'jpeg'], 90)
ON CONFLICT (document_code) DO NOTHING;

-- Link document requirements to services (Funeral Cover)
INSERT INTO service_document_requirements (service_type_id, document_type_id, is_required, display_order)
SELECT st.id, dt.id, true, 1
FROM service_types st, document_types dt
WHERE st.service_code = 'FUNERAL' AND dt.document_code = 'ID_DOCUMENT'
ON CONFLICT DO NOTHING;

INSERT INTO service_document_requirements (service_type_id, document_type_id, is_required, display_order)
SELECT st.id, dt.id, true, 2
FROM service_types st, document_types dt
WHERE st.service_code = 'FUNERAL' AND dt.document_code = 'PROOF_OF_ADDRESS'
ON CONFLICT DO NOTHING;

-- Link document requirements to services (Life Insurance)
INSERT INTO service_document_requirements (service_type_id, document_type_id, is_required, is_conditional, condition_description, display_order)
SELECT st.id, dt.id, true, false, NULL, 1
FROM service_types st, document_types dt
WHERE st.service_code = 'LIFE' AND dt.document_code = 'ID_DOCUMENT'
ON CONFLICT DO NOTHING;

INSERT INTO service_document_requirements (service_type_id, document_type_id, is_required, is_conditional, condition_description, display_order)
SELECT st.id, dt.id, true, false, NULL, 2
FROM service_types st, document_types dt
WHERE st.service_code = 'LIFE' AND dt.document_code = 'PROOF_OF_ADDRESS'
ON CONFLICT DO NOTHING;

INSERT INTO service_document_requirements (service_type_id, document_type_id, is_required, is_conditional, condition_description, display_order)
SELECT st.id, dt.id, false, true, 'Required for coverage above R500,000 or age above 50', 3
FROM service_types st, document_types dt
WHERE st.service_code = 'LIFE' AND dt.document_code = 'MEDICAL_CERTIFICATE'
ON CONFLICT DO NOTHING;

