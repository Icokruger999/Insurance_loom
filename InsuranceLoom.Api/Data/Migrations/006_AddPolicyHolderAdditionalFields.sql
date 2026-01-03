-- Migration: Add additional fields to policy_holders table
-- Description: Adds fields for personal information, employment, and agency details

ALTER TABLE policy_holders
ADD COLUMN IF NOT EXISTS middle_name VARCHAR(100),
ADD COLUMN IF NOT EXISTS birthplace VARCHAR(200),
ADD COLUMN IF NOT EXISTS sex VARCHAR(20),
ADD COLUMN IF NOT EXISTS civil_status VARCHAR(50),
ADD COLUMN IF NOT EXISTS occupation VARCHAR(200),
ADD COLUMN IF NOT EXISTS monthly_income DECIMAL(18, 2),
ADD COLUMN IF NOT EXISTS monthly_expenses DECIMAL(18, 2),
ADD COLUMN IF NOT EXISTS employment_type VARCHAR(50),
ADD COLUMN IF NOT EXISTS income_tax_number VARCHAR(50),
ADD COLUMN IF NOT EXISTS employment_start_date DATE,
ADD COLUMN IF NOT EXISTS employment_end_date DATE,
ADD COLUMN IF NOT EXISTS agency_name VARCHAR(200),
ADD COLUMN IF NOT EXISTS agency_contact_no VARCHAR(50),
ADD COLUMN IF NOT EXISTS agency_address TEXT,
ADD COLUMN IF NOT EXISTS agency_email VARCHAR(255),
ADD COLUMN IF NOT EXISTS agency_signatory VARCHAR(200);

-- Add comments for documentation
COMMENT ON COLUMN policy_holders.middle_name IS 'Middle name of the policy holder';
COMMENT ON COLUMN policy_holders.birthplace IS 'Place of birth';
COMMENT ON COLUMN policy_holders.sex IS 'Gender/Sex (Male, Female, Other)';
COMMENT ON COLUMN policy_holders.civil_status IS 'Marital status (Single, Married, Divorced, Widowed)';
COMMENT ON COLUMN policy_holders.occupation IS 'Occupation/Job title';
COMMENT ON COLUMN policy_holders.monthly_income IS 'Monthly income amount';
COMMENT ON COLUMN policy_holders.monthly_expenses IS 'Monthly expenses amount';
COMMENT ON COLUMN policy_holders.employment_type IS 'Employment type (Employee, Self Employed)';
COMMENT ON COLUMN policy_holders.income_tax_number IS 'Income tax number';
COMMENT ON COLUMN policy_holders.employment_start_date IS 'Employment start date';
COMMENT ON COLUMN policy_holders.employment_end_date IS 'Employment end date';
COMMENT ON COLUMN policy_holders.agency_name IS 'Agency/Employer name';
COMMENT ON COLUMN policy_holders.agency_contact_no IS 'Agency contact number';
COMMENT ON COLUMN policy_holders.agency_address IS 'Agency/Employer address';
COMMENT ON COLUMN policy_holders.agency_email IS 'Agency/Employer email';
COMMENT ON COLUMN policy_holders.agency_signatory IS 'Authorized signatory name';

