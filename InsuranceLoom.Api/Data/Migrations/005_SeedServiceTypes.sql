-- Seed Service Types (Insurance Products)
INSERT INTO service_types (id, service_code, service_name, description, is_active, created_at)
VALUES
    (gen_random_uuid(), 'FUNERAL', 'Funeral Cover', 'Comprehensive funeral insurance coverage for individuals and families', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'PROPERTY', 'Property Insurance', 'Home and property insurance coverage', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'VEHICLE', 'Vehicle Insurance', 'Motor vehicle and car insurance coverage', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'LIFE', 'Life Insurance', 'Life insurance and term life coverage', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'HEALTH', 'Health Insurance', 'Medical and health insurance coverage', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'DISABILITY', 'Disability Insurance', 'Income protection and disability coverage', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'TRAVEL', 'Travel Insurance', 'Travel and holiday insurance coverage', true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'BUSINESS', 'Business Insurance', 'Commercial and business insurance coverage', true, CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

