-- Test/Dummy Policies Data for Dashboard Testing
-- This script creates test policies with different statuses, dates, brokers, and regions
-- These can be cleaned up later with: DELETE FROM policy_approvals WHERE id IN (SELECT id FROM policy_approvals WHERE review_notes LIKE '%TEST DATA%');
-- DELETE FROM policies WHERE policy_number LIKE 'TEST-%';

-- Add address columns to policy_holders table (replacing the text address column)
ALTER TABLE policy_holders
ADD COLUMN IF NOT EXISTS street_address VARCHAR(255),
ADD COLUMN IF NOT EXISTS city VARCHAR(100),
ADD COLUMN IF NOT EXISTS province VARCHAR(100),
ADD COLUMN IF NOT EXISTS postal_code VARCHAR(20),
ADD COLUMN IF NOT EXISTS country VARCHAR(100) DEFAULT 'South Africa';

-- Drop the old address column if it exists
ALTER TABLE policy_holders DROP COLUMN IF EXISTS address;

DO $$
DECLARE
    v_broker1_id UUID;
    v_broker2_id UUID;
    v_broker3_id UUID;
    v_manager_id UUID;
    v_service_type1_id UUID;
    v_service_type2_id UUID;
    v_policy_holder1_id UUID;
    v_policy_holder2_id UUID;
    v_policy_holder3_id UUID;
    v_policy_holder4_id UUID;
    v_policy_holder5_id UUID;
    v_user1_id UUID;
    v_user2_id UUID;
    v_user3_id UUID;
    v_user4_id UUID;
    v_user5_id UUID;
    v_policy_id UUID;
    v_approval_id UUID;
BEGIN
    -- Get existing broker IDs (first 3 brokers)
    SELECT id INTO v_broker1_id FROM brokers LIMIT 1 OFFSET 0;
    SELECT id INTO v_broker2_id FROM brokers LIMIT 1 OFFSET 1;
    SELECT id INTO v_broker3_id FROM brokers LIMIT 1 OFFSET 2;
    
    -- Get manager ID
    SELECT id INTO v_manager_id FROM managers LIMIT 1;
    
    -- Get service types
    SELECT id INTO v_service_type1_id FROM service_types WHERE service_name LIKE '%Funeral%' OR service_name LIKE '%Life%' LIMIT 1;
    SELECT id INTO v_service_type2_id FROM service_types WHERE service_name LIKE '%Property%' OR service_name LIKE '%Home%' LIMIT 1;
    IF v_service_type1_id IS NULL THEN
        SELECT id INTO v_service_type1_id FROM service_types LIMIT 1;
    END IF;
    IF v_service_type2_id IS NULL THEN
        SELECT id INTO v_service_type2_id FROM service_types LIMIT 1 OFFSET 1;
    END IF;
    
    -- Create test policy holders and users if brokers exist
    IF v_broker1_id IS NOT NULL AND v_broker2_id IS NOT NULL THEN
        -- Create test users for policy holders
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES 
            (gen_random_uuid(), 'test.policyholder1@test.com', '$2a$11$PlaceholderHash', 'PolicyHolder', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), 'test.policyholder2@test.com', '$2a$11$PlaceholderHash', 'PolicyHolder', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), 'test.policyholder3@test.com', '$2a$11$PlaceholderHash', 'PolicyHolder', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), 'test.policyholder4@test.com', '$2a$11$PlaceholderHash', 'PolicyHolder', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), 'test.policyholder5@test.com', '$2a$11$PlaceholderHash', 'PolicyHolder', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ON CONFLICT (email) DO NOTHING
        RETURNING id INTO v_user1_id;
        
        SELECT id INTO v_user1_id FROM users WHERE email = 'test.policyholder1@test.com';
        SELECT id INTO v_user2_id FROM users WHERE email = 'test.policyholder2@test.com';
        SELECT id INTO v_user3_id FROM users WHERE email = 'test.policyholder3@test.com';
        SELECT id INTO v_user4_id FROM users WHERE email = 'test.policyholder4@test.com';
        SELECT id INTO v_user5_id FROM users WHERE email = 'test.policyholder5@test.com';
        
        -- Create test policy holders
        INSERT INTO policy_holders (id, user_id, first_name, last_name, phone, id_number, date_of_birth, street_address, city, province, postal_code, country, is_active, created_at, updated_at)
        VALUES 
            (gen_random_uuid(), v_user1_id, 'John', 'Doe', '0821234567', '8501015800081', '1985-01-01', '123 Main St', 'Cape Town', 'Western Cape', '8001', 'South Africa', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), v_user2_id, 'Jane', 'Smith', '0822345678', '9002025800082', '1990-02-02', '456 Oak Ave', 'Johannesburg', 'Gauteng', '2000', 'South Africa', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), v_user3_id, 'Mike', 'Johnson', '0823456789', '8803035800083', '1988-03-03', '789 Pine Rd', 'Durban', 'KwaZulu-Natal', '4001', 'South Africa', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), v_user4_id, 'Sarah', 'Williams', '0824567890', '9204045800084', '1992-04-04', '321 Elm St', 'Pretoria', 'Gauteng', '0001', 'South Africa', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
            (gen_random_uuid(), v_user5_id, 'David', 'Brown', '0825678901', '8605055800085', '1986-05-05', '654 Maple Dr', 'Bloemfontein', 'Free State', '9301', 'South Africa', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ON CONFLICT DO NOTHING
        RETURNING id INTO v_policy_holder1_id;
        
        SELECT id INTO v_policy_holder1_id FROM policy_holders WHERE email = 'test.policyholder1@test.com';
        SELECT id INTO v_policy_holder2_id FROM policy_holders WHERE email = 'test.policyholder2@test.com';
        SELECT id INTO v_policy_holder3_id FROM policy_holders WHERE email = 'test.policyholder3@test.com';
        SELECT id INTO v_policy_holder4_id FROM policy_holders WHERE email = 'test.policyholder4@test.com';
        SELECT id INTO v_policy_holder5_id FROM policy_holders WHERE email = 'test.policyholder5@test.com';
        
        -- Create test policies with different statuses and dates
        -- Policy 1: Approved (Cape Town, 2 months ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-001', v_policy_holder1_id, v_broker1_id, v_service_type1_id, 500000.00, 850.00, CURRENT_DATE - INTERVAL '2 months', 'Approved', CURRENT_TIMESTAMP - INTERVAL '2 months', CURRENT_TIMESTAMP - INTERVAL '2 months')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, approved_by, approved_date, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker1_id, v_policy_holder1_id, 'Approved', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '2 months', CURRENT_TIMESTAMP - INTERVAL '2 months', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '2 months' + INTERVAL '2 days', 'TEST DATA - Approved policy')
        RETURNING id INTO v_approval_id;
        
        -- Policy 2: Pending (Johannesburg, 1 week ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-002', v_policy_holder2_id, v_broker2_id, v_service_type2_id, 750000.00, 1200.00, CURRENT_DATE + INTERVAL '1 month', 'PendingSubmission', CURRENT_TIMESTAMP - INTERVAL '1 week', CURRENT_TIMESTAMP - INTERVAL '1 week')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker2_id, v_policy_holder2_id, 'Pending', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '1 week', CURRENT_TIMESTAMP - INTERVAL '1 week', 'TEST DATA - Pending approval')
        RETURNING id INTO v_approval_id;
        
        -- Policy 3: Rejected (Durban, 1 month ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-003', v_policy_holder3_id, v_broker1_id, v_service_type1_id, 300000.00, 650.00, CURRENT_DATE - INTERVAL '1 month', 'Rejected', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, rejected_by, rejected_date, rejection_reason, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker1_id, v_policy_holder3_id, 'Rejected', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '1 month' + INTERVAL '5 days', 'Missing required documentation', 'TEST DATA - Rejected policy')
        RETURNING id INTO v_approval_id;
        
        -- Policy 4: Approved (Pretoria, 3 months ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-004', v_policy_holder4_id, v_broker2_id, v_service_type1_id, 1000000.00, 1500.00, CURRENT_DATE - INTERVAL '3 months', 'Active', CURRENT_TIMESTAMP - INTERVAL '3 months', CURRENT_TIMESTAMP - INTERVAL '3 months')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, approved_by, approved_date, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker2_id, v_policy_holder4_id, 'Approved', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '3 months', CURRENT_TIMESTAMP - INTERVAL '3 months', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '3 months' + INTERVAL '3 days', 'TEST DATA - Active policy')
        RETURNING id INTO v_approval_id;
        
        -- Policy 5: Under Review (Bloemfontein, 2 days ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-005', v_policy_holder5_id, v_broker3_id, v_service_type2_id, 600000.00, 950.00, CURRENT_DATE + INTERVAL '2 weeks', 'UnderReview', CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP - INTERVAL '2 days')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker3_id, v_policy_holder5_id, 'UnderReview', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP - INTERVAL '2 days', 'TEST DATA - Under review')
        RETURNING id INTO v_approval_id;
        
        -- Policy 6: Draft (Cape Town, today) - no approval record
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-006', v_policy_holder1_id, v_broker1_id, v_service_type1_id, 400000.00, 750.00, CURRENT_DATE + INTERVAL '1 month', 'Draft', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING id INTO v_policy_id;
        
        -- Policy 7: Changes Required (Johannesburg, 5 days ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-007', v_policy_holder2_id, v_broker2_id, v_service_type1_id, 550000.00, 900.00, CURRENT_DATE + INTERVAL '3 weeks', 'ChangesRequired', CURRENT_TIMESTAMP - INTERVAL '5 days', CURRENT_TIMESTAMP - INTERVAL '5 days')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, changes_requested_by, changes_requested_date, changes_required, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker2_id, v_policy_holder2_id, 'RequiresChanges', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '5 days', CURRENT_TIMESTAMP - INTERVAL '5 days', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '3 days', 'Please update beneficiary information', 'TEST DATA - Changes required')
        RETURNING id INTO v_approval_id;
        
        -- Policy 8: Approved (Durban, 4 months ago)
        INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, status, created_at, updated_at)
        VALUES (gen_random_uuid(), 'TEST-POL-008', v_policy_holder3_id, v_broker3_id, v_service_type2_id, 800000.00, 1100.00, CURRENT_DATE - INTERVAL '4 months', 'Active', CURRENT_TIMESTAMP - INTERVAL '4 months', CURRENT_TIMESTAMP - INTERVAL '4 months')
        RETURNING id INTO v_policy_id;
        
        INSERT INTO policy_approvals (id, policy_id, broker_id, policy_holder_id, status, assigned_manager_id, assigned_date, submitted_date, approved_by, approved_date, review_notes)
        VALUES (gen_random_uuid(), v_policy_id, v_broker3_id, v_policy_holder3_id, 'Approved', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '4 months', CURRENT_TIMESTAMP - INTERVAL '4 months', v_manager_id, CURRENT_TIMESTAMP - INTERVAL '4 months' + INTERVAL '4 days', 'TEST DATA - Approved policy')
        RETURNING id INTO v_approval_id;
        
    END IF;
END $$;

-- Note: To clean up test data later, run:
-- DELETE FROM policy_approval_history WHERE approval_id IN (SELECT id FROM policy_approvals WHERE review_notes LIKE '%TEST DATA%');
-- DELETE FROM policy_approvals WHERE review_notes LIKE '%TEST DATA%';
-- DELETE FROM policies WHERE policy_number LIKE 'TEST-%';
-- DELETE FROM policy_holders WHERE email LIKE 'test.policyholder%@test.com';
-- DELETE FROM users WHERE email LIKE 'test.policyholder%@test.com';

