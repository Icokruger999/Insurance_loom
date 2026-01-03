-- Migration: Create beneficiaries table
-- Description: Stores beneficiary information for policy holders

CREATE TABLE IF NOT EXISTS beneficiaries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    policy_holder_id UUID NOT NULL REFERENCES policy_holders(id) ON DELETE CASCADE,
    policy_id UUID REFERENCES policies(id) ON DELETE SET NULL,
    full_name VARCHAR(200) NOT NULL,
    date_of_birth DATE,
    age INTEGER,
    mobile VARCHAR(50),
    email VARCHAR(255),
    relationship VARCHAR(100),
    type VARCHAR(50), -- Revocable, Irrevocable
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_beneficiaries_policy_holder_id ON beneficiaries(policy_holder_id);
CREATE INDEX IF NOT EXISTS idx_beneficiaries_policy_id ON beneficiaries(policy_id);

-- Add comments for documentation
COMMENT ON TABLE beneficiaries IS 'Stores beneficiary information for policy holders';
COMMENT ON COLUMN beneficiaries.policy_holder_id IS 'Reference to the policy holder';
COMMENT ON COLUMN beneficiaries.policy_id IS 'Optional reference to a specific policy';
COMMENT ON COLUMN beneficiaries.full_name IS 'Full name of the beneficiary';
COMMENT ON COLUMN beneficiaries.date_of_birth IS 'Date of birth of the beneficiary';
COMMENT ON COLUMN beneficiaries.age IS 'Age of the beneficiary';
COMMENT ON COLUMN beneficiaries.mobile IS 'Mobile phone number';
COMMENT ON COLUMN beneficiaries.email IS 'Email address';
COMMENT ON COLUMN beneficiaries.relationship IS 'Relationship to policy holder (e.g., Spouse, Child)';
COMMENT ON COLUMN beneficiaries.type IS 'Beneficiary type (Revocable, Irrevocable)';

