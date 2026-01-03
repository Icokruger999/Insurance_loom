-- Migration: Create dependents table and update beneficiaries table
-- Description: Creates a table for dependents (children, spouse, etc.) covered under the policy
--              and adds is_primary flag to beneficiaries table to identify primary beneficiary

-- Create dependents table for people covered under the policy (children, spouse, etc.)
CREATE TABLE IF NOT EXISTS dependents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    policy_holder_id UUID NOT NULL REFERENCES policy_holders(id) ON DELETE CASCADE,
    policy_id UUID REFERENCES policies(id) ON DELETE SET NULL,
    first_name VARCHAR(200) NOT NULL,
    last_name VARCHAR(200) NOT NULL,
    middle_name VARCHAR(100),
    id_number VARCHAR(50),
    date_of_birth DATE,
    relationship VARCHAR(100), -- Spouse, Child, Parent, etc.
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_dependents_policy_holder_id ON dependents(policy_holder_id);
CREATE INDEX IF NOT EXISTS idx_dependents_policy_id ON dependents(policy_id);

-- Add is_primary column to beneficiaries table to identify primary beneficiary
ALTER TABLE beneficiaries
ADD COLUMN IF NOT EXISTS is_primary BOOLEAN DEFAULT false;

-- Add comments for documentation
COMMENT ON TABLE dependents IS 'Stores dependents (children, spouse, etc.) covered under the policy';
COMMENT ON COLUMN dependents.policy_holder_id IS 'Reference to the primary/main policy holder';
COMMENT ON COLUMN dependents.policy_id IS 'Optional reference to a specific policy';
COMMENT ON COLUMN dependents.first_name IS 'First name of the dependent';
COMMENT ON COLUMN dependents.last_name IS 'Last name of the dependent';
COMMENT ON COLUMN dependents.middle_name IS 'Middle name of the dependent';
COMMENT ON COLUMN dependents.id_number IS 'ID number of the dependent';
COMMENT ON COLUMN dependents.date_of_birth IS 'Date of birth of the dependent';
COMMENT ON COLUMN dependents.relationship IS 'Relationship to primary policy holder (e.g., Spouse, Child, Parent)';
COMMENT ON COLUMN dependents.is_active IS 'Whether the dependent is currently active/covered';
COMMENT ON COLUMN beneficiaries.is_primary IS 'Indicates if this is the primary beneficiary';

