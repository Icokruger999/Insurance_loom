-- Add company_id column to managers table
ALTER TABLE managers ADD COLUMN IF NOT EXISTS company_id UUID REFERENCES companies(id) ON DELETE SET NULL;

-- Create index on company_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_managers_company_id ON managers(company_id);

-- Insert initial manager: Ico Kruger for Astutetech Data
-- First, get the company ID for Astutetech Data
DO $$
DECLARE
    v_company_id UUID;
    v_user_id UUID;
BEGIN
    -- Get the company ID for Astutetech Data
    SELECT id INTO v_company_id FROM companies WHERE name = 'Astutetech Data' LIMIT 1;
    
    IF v_company_id IS NULL THEN
        RAISE EXCEPTION 'Company "Astutetech Data" not found. Please ensure the company exists.';
    END IF;
    
    -- Check if manager already exists
    IF NOT EXISTS (SELECT 1 FROM managers WHERE email = 'ico@astutetech.co.za') THEN
        -- Create a user for the manager (if not exists)
        IF NOT EXISTS (SELECT 1 FROM users WHERE email = 'ico@astutetech.co.za') THEN
            INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
            VALUES (
                gen_random_uuid(),
                'ico@astutetech.co.za',
                '$2a$11$PlaceholderPasswordHash', -- Temporary password hash, should be updated
                'Manager',
                true,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            )
            RETURNING id INTO v_user_id;
        ELSE
            SELECT id INTO v_user_id FROM users WHERE email = 'ico@astutetech.co.za';
        END IF;
        
        -- Insert the manager
        INSERT INTO managers (id, user_id, company_id, email, first_name, last_name, is_active, can_approve_policies, can_manage_brokers, can_view_reports, created_at, updated_at)
        VALUES (
            gen_random_uuid(),
            v_user_id,
            v_company_id,
            'ico@astutetech.co.za',
            'Ico',
            'Kruger',
            true,
            true,
            true,
            true,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        );
    END IF;
END $$;

