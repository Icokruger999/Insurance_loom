# Company Management Setup

## Overview

The system now includes company validation for broker registration. Brokers must select from existing companies or create new ones during registration.

## Database Setup

### 1. Run Migration Script

Execute the migration script on your PostgreSQL database:

```bash
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom -f InsuranceLoom.Api/Data/Migrations/002_AddCompaniesTable.sql
```

Or manually run the SQL:

```sql
-- Add companies table
CREATE TABLE IF NOT EXISTS companies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL UNIQUE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_companies_name ON companies(name);
CREATE INDEX IF NOT EXISTS idx_companies_is_active ON companies(is_active);

-- Insert initial companies
INSERT INTO companies (id, name, is_active, created_at, updated_at)
VALUES 
    (gen_random_uuid(), 'Astutetech Data', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    (gen_random_uuid(), 'Pogo Group', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (name) DO NOTHING;
```

## Features

### 1. Company Validation on Broker Registration

- When a broker registers with a company name, the system checks if the company exists in the `companies` table
- If the company doesn't exist and `createCompanyIfNotExists` is `false`, registration fails with an error message
- If `createCompanyIfNotExists` is `true`, the company is automatically created

### 2. Company Lookup

- The registration form includes a company input field with autocomplete (datalist)
- Companies are loaded from the API and displayed as suggestions
- Brokers can type to filter or type a new company name

### 3. Company Creation

- Brokers can check "Create company if it doesn't exist" to automatically create the company
- Companies can also be created separately via the `/api/company` endpoint (requires authentication)

## API Endpoints

### Get Companies
- `GET /api/company?activeOnly=true` - Get list of companies (requires authentication)
- `GET /api/company/{id}` - Get specific company (requires authentication)

### Create Company
- `POST /api/company` - Create a new company (requires Broker or Manager role)
  ```json
  {
    "name": "Company Name"
  }
  ```

### Broker Registration (Updated)
- `POST /api/auth/broker/register` - Register a new broker
  ```json
  {
    "email": "broker@example.com",
    "password": "password123",
    "firstName": "John",
    "lastName": "Doe",
    "companyName": "Astutetech Data",
    "createCompanyIfNotExists": false,
    "phone": "1234567890",
    "licenseNumber": "LIC123"
  }
  ```

## Frontend Changes

1. **Company Input Field:**
   - Added datalist for company autocomplete
   - Added checkbox to enable company creation if it doesn't exist
   - Companies are loaded on page load

2. **Registration Flow:**
   - User can select from existing companies or type a new one
   - If typing a new company, check the "Create company if it doesn't exist" checkbox
   - If company doesn't exist and checkbox is unchecked, registration fails with clear error message

## Deployment Steps

1. **Run Database Migration:**
   ```bash
   psql -h YOUR_RDS_HOST -U postgres -d insuranceloom -f InsuranceLoom.Api/Data/Migrations/002_AddCompaniesTable.sql
   ```

2. **Deploy API Code:**
   ```bash
   cd ~/Insurance_loom
   git pull origin main
   cd InsuranceLoom.Api
   ./deploy.sh
   ```

3. **Verify Companies Table:**
   ```sql
   SELECT * FROM companies;
   ```
   Should show:
   - Astutetech Data
   - Pogo Group

## Testing

1. **Test Company Validation:**
   - Try registering a broker with an existing company (should work)
   - Try registering a broker with a non-existent company without checkbox (should fail)
   - Try registering a broker with a non-existent company with checkbox checked (should create company and register)

2. **Test Company Creation:**
   ```bash
   curl -X POST https://api.insuranceloom.com/api/company \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -d '{"name": "New Company Name"}'
   ```

3. **Test Company List:**
   ```bash
   curl -X GET https://api.insuranceloom.com/api/company?activeOnly=true \
     -H "Authorization: Bearer YOUR_TOKEN"
   ```

## Notes

- Company names are case-insensitive for matching
- Company names are trimmed before validation/creation
- Duplicate company names are prevented (unique constraint on name)
- Inactive companies are excluded from validation (only active companies can be selected)
- Brokers and Managers can create companies via the API endpoint

