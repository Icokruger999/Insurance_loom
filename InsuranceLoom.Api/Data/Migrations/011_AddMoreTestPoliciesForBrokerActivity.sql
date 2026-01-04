-- Add More Test Policies for Broker Activity Dashboard
-- This script creates additional test policies with various statuses and dates
-- to populate the Broker Activity Dashboard

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
    v_broker_ids UUID[];
    v_service_type_ids UUID[];
    v_policy_holder_ids UUID[];
    v_user_ids UUID[];
    v_policy_id UUID;
    v_approval_id UUID;
    v_broker_id UUID;
    v_service_type_id UUID;
    v_policy_holder_id UUID;
    v_user_id UUID;
    v_statuses TEXT[] := ARRAY['Active', 'Approved', 'PendingSubmission', 'Submitted', 'UnderReview', 'Rejected', 'Draft', 'Cancelled', 'ChangesRequired'];
    v_service_types TEXT[] := ARRAY['FUNERAL', 'LIFE', 'HEALTH', 'DISABILITY', 'INCOME'];
    i INT;
    j INT;
    v_start_date DATE;
    v_end_date DATE;
    v_status TEXT;
    v_coverage DECIMAL;
    v_premium DECIMAL;
BEGIN
    -- Get all active broker IDs
    SELECT ARRAY_AGG(id) INTO v_broker_ids FROM brokers WHERE is_active = true LIMIT 10;
    
    -- Get service type IDs
    SELECT ARRAY_AGG(id) INTO v_service_type_ids FROM service_types LIMIT 5;
    
    -- If no brokers exist, exit
    IF v_broker_ids IS NULL OR array_length(v_broker_ids, 1) = 0 THEN
        RAISE NOTICE 'No active brokers found. Please run migration 010_AddTestBrokers.sql first.';
        RETURN;
    END IF;
    
    -- Create additional policy holders and users
    FOR i IN 1..20 LOOP
        -- Create user
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            'test.holder' || i || '@test.com',
            '$2a$11$PlaceholderHash',
            'PolicyHolder',
            true,
            CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '365 days'),
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_user_id;
        
        v_user_ids := array_append(v_user_ids, v_user_id);
        
        -- Create policy holder
        INSERT INTO policy_holders (id, user_id, policy_number, first_name, last_name, phone, street_address, city, province, postal_code, country, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            'TEST-PH-' || LPAD(i::TEXT, 4, '0'),
            'Test' || i,
            'Holder' || i,
            '082' || LPAD((1000000 + i)::TEXT, 7, '0'),
            CASE (i % 4)
                WHEN 0 THEN '123 Test St'
                WHEN 1 THEN '456 Test Ave'
                WHEN 2 THEN '789 Test Rd'
                ELSE '321 Test Dr'
            END,
            CASE (i % 4)
                WHEN 0 THEN 'Cape Town'
                WHEN 1 THEN 'Johannesburg'
                WHEN 2 THEN 'Durban'
                ELSE 'Pretoria'
            END,
            CASE (i % 4)
                WHEN 0 THEN 'Western Cape'
                WHEN 1 THEN 'Gauteng'
                WHEN 2 THEN 'KwaZulu-Natal'
                ELSE 'Gauteng'
            END,
            CASE (i % 4)
                WHEN 0 THEN '8001'
                WHEN 1 THEN '2001'
                WHEN 2 THEN '4001'
                ELSE '0001'
            END,
            'South Africa',
            true,
            CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '365 days'),
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_policy_holder_id;
        
        v_policy_holder_ids := array_append(v_policy_holder_ids, v_policy_holder_id);
    END LOOP;
    
    -- Create policies for each broker
    FOR i IN 1..array_length(v_broker_ids, 1) LOOP
        v_broker_id := v_broker_ids[i];
        
        -- Create 5-15 policies per broker
        FOR j IN 1..(5 + (RANDOM() * 10)::INT) LOOP
            -- Select random service type
            v_service_type_id := v_service_type_ids[1 + (RANDOM() * (array_length(v_service_type_ids, 1) - 1))::INT];
            
            -- Select random policy holder
            v_policy_holder_id := v_policy_holder_ids[1 + (RANDOM() * (array_length(v_policy_holder_ids, 1) - 1))::INT];
            
            -- Select random status
            v_status := v_statuses[1 + (RANDOM() * (array_length(v_statuses, 1) - 1))::INT];
            
            -- Generate dates
            v_start_date := CURRENT_DATE - (RANDOM() * INTERVAL '730 days');
            v_end_date := v_start_date + (1 + RANDOM() * 10)::INT * INTERVAL '1 year';
            
            -- Generate amounts
            v_coverage := 50000 + (RANDOM() * 950000)::DECIMAL;
            v_premium := 100 + (RANDOM() * 900)::DECIMAL;
            
            -- Create policy
            INSERT INTO policies (id, policy_number, policy_holder_id, broker_id, service_type_id, coverage_amount, premium_amount, start_date, end_date, status, created_at, updated_at)
            VALUES (
                gen_random_uuid(),
                'POL-' || LPAD((i * 100 + j)::TEXT, 6, '0'),
                v_policy_holder_id,
                v_broker_id,
                v_service_type_id,
                v_coverage,
                v_premium,
                v_start_date,
                v_end_date,
                v_status,
                CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '180 days'),
                CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '180 days')
            )
            RETURNING id INTO v_policy_id;
            
            -- Create approval record for pending/under review policies
            IF v_status IN ('PendingSubmission', 'Submitted', 'UnderReview', 'Approved', 'Rejected', 'ChangesRequired') THEN
                INSERT INTO policy_approvals (
                    id, policy_id, broker_id, policy_holder_id, status, submitted_date, 
                    assigned_manager_id, assigned_date, created_at, updated_at
                )
                VALUES (
                    gen_random_uuid(),
                    v_policy_id,
                    v_broker_id,
                    v_policy_holder_id,
                    CASE v_status
                        WHEN 'Approved' THEN 'Approved'
                        WHEN 'Rejected' THEN 'Rejected'
                        WHEN 'UnderReview' THEN 'UnderReview'
                        WHEN 'ChangesRequired' THEN 'RequiresChanges'
                        ELSE 'Pending'
                    END,
                    CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '30 days'),
                    (SELECT id FROM managers LIMIT 1),
                    CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '30 days'),
                    CURRENT_TIMESTAMP - (RANDOM() * INTERVAL '30 days'),
                    CURRENT_TIMESTAMP
                );
            END IF;
        END LOOP;
    END LOOP;
    
    RAISE NOTICE 'Successfully created test policies for broker activity dashboard.';
END $$;

