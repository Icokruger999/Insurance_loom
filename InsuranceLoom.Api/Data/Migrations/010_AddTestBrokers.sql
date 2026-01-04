-- Add Test Brokers for Dashboard Testing
-- This script creates test brokers with different regions
-- These can be cleaned up later with: DELETE FROM brokers WHERE agent_number LIKE 'TEST-%';

DO $$
DECLARE
    v_company1_id UUID;
    v_company2_id UUID;
    v_user_id UUID;
    v_broker_id UUID;
    v_agent_number VARCHAR(50);
BEGIN
    -- Get company IDs
    SELECT id INTO v_company1_id FROM companies WHERE name = 'Astutetech Data' LIMIT 1;
    SELECT id INTO v_company2_id FROM companies WHERE name = 'Pogo Group' LIMIT 1;
    
    -- If companies don't exist, create them
    IF v_company1_id IS NULL THEN
        INSERT INTO companies (id, name, registration_number, address, phone, email, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'Astutetech Data', 'REG001', '21 Sparrow, Kraaifontein, Langeberg Ridge, Cape Town', '0793309356', 'info@astutetech.co.za', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING id INTO v_company1_id;
    END IF;
    
    IF v_company2_id IS NULL THEN
        INSERT INTO companies (id, name, registration_number, address, phone, email, is_active, created_at, updated_at)
        VALUES (gen_random_uuid(), 'Pogo Group', 'REG002', '123 Main St, Johannesburg', '0111234567', 'info@pogogroup.co.za', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING id INTO v_company2_id;
    END IF;
    
    -- Broker 1: Cape Town
    IF NOT EXISTS (SELECT 1 FROM brokers WHERE agent_number = 'TEST-BRK-001') THEN
        -- Create user
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            'broker1.capetown@test.com',
            '$2a$11$PlaceholderPasswordHash', -- Temporary password: should be updated
            'Broker',
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_user_id;
        
        -- Create broker
        INSERT INTO brokers (id, user_id, agent_number, first_name, last_name, phone, company_name, license_number, commission_rate, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            'TEST-BRK-001',
            'John',
            'Smith',
            '0821111111',
            'Astutetech Data',
            'LIC001',
            0.00,
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        );
    END IF;
    
    -- Broker 2: Johannesburg
    IF NOT EXISTS (SELECT 1 FROM brokers WHERE agent_number = 'TEST-BRK-002') THEN
        -- Create user
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            'broker2.johannesburg@test.com',
            '$2a$11$PlaceholderPasswordHash',
            'Broker',
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_user_id;
        
        -- Create broker
        INSERT INTO brokers (id, user_id, agent_number, first_name, last_name, email, phone, company_name, license_number, commission_rate, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            'TEST-BRK-002',
            'Sarah',
            'Johnson',
            'broker2.johannesburg@test.com',
            '0822222222',
            'Pogo Group',
            'LIC002',
            0.00,
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        );
    END IF;
    
    -- Broker 3: Durban
    IF NOT EXISTS (SELECT 1 FROM brokers WHERE agent_number = 'TEST-BRK-003') THEN
        -- Create user
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            'broker3.durban@test.com',
            '$2a$11$PlaceholderPasswordHash',
            'Broker',
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_user_id;
        
        -- Create broker
        INSERT INTO brokers (id, user_id, agent_number, first_name, last_name, email, phone, company_name, license_number, commission_rate, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            'TEST-BRK-003',
            'Mike',
            'Williams',
            'broker3.durban@test.com',
            '0823333333',
            'Astutetech Data',
            'LIC003',
            0.00,
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        );
    END IF;
    
    -- Broker 4: Pretoria
    IF NOT EXISTS (SELECT 1 FROM brokers WHERE agent_number = 'TEST-BRK-004') THEN
        -- Create user
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            'broker4.pretoria@test.com',
            '$2a$11$PlaceholderPasswordHash',
            'Broker',
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_user_id;
        
        -- Create broker
        INSERT INTO brokers (id, user_id, agent_number, first_name, last_name, email, phone, company_name, license_number, commission_rate, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            'TEST-BRK-004',
            'Lisa',
            'Brown',
            'broker4.pretoria@test.com',
            '0824444444',
            'Pogo Group',
            'LIC004',
            0.00,
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        );
    END IF;
    
    -- Broker 5: Bloemfontein
    IF NOT EXISTS (SELECT 1 FROM brokers WHERE agent_number = 'TEST-BRK-005') THEN
        -- Create user
        INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            'broker5.bloemfontein@test.com',
            '$2a$11$PlaceholderPasswordHash',
            'Broker',
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        )
        RETURNING id INTO v_user_id;
        
        -- Create broker
        INSERT INTO brokers (id, user_id, agent_number, first_name, last_name, email, phone, company_name, license_number, commission_rate, is_active, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            'TEST-BRK-005',
            'David',
            'Miller',
            'broker5.bloemfontein@test.com',
            '0825555555',
            'Astutetech Data',
            'LIC005',
            0.00,
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        );
    END IF;
    
    -- Activate all test brokers (set is_active = true and ensure they're approved)
    UPDATE brokers 
    SET is_active = true, updated_at = CURRENT_TIMESTAMP
    WHERE agent_number LIKE 'TEST-BRK-%' AND is_active = false;
    
END $$;

-- Note: To clean up test brokers later, run:
-- DELETE FROM brokers WHERE agent_number LIKE 'TEST-BRK-%';
-- DELETE FROM users WHERE email LIKE 'broker%.test.com';

