# Manager-Company Association Setup

## Overview

Managers are now associated with companies, and brokers must provide their manager's email during registration. The manager receives the approval request email.

## Database Setup

### 1. Run Migration Script

Execute the migration script on your PostgreSQL database:

```bash
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom -f InsuranceLoom.Api/Data/Migrations/003_AddManagerCompanyAndInitialManager.sql
```

Or manually run the SQL:

```sql
-- Add company_id column to managers table
ALTER TABLE managers ADD COLUMN IF NOT EXISTS company_id UUID REFERENCES companies(id) ON DELETE SET NULL;

-- Create index
CREATE INDEX IF NOT EXISTS idx_managers_company_id ON managers(company_id);

-- Insert initial manager: Ico Kruger for Astutetech Data
-- (See migration script for full details)
```

## Initial Manager

- **Name:** Ico Kruger
- **Email:** ico@astutetech.co.za
- **Company:** Astutetech Data
- **Permissions:** Can approve policies, manage brokers, view reports

## Broker Registration Changes

1. **Manager Email Field:** Required field in broker registration
2. **Validation:** Manager email must:
   - Exist in the managers table
   - Be active
   - Be associated with the broker's selected company

3. **Approval Workflow:**
   - Approval email is sent to the broker's manager (not a global approver)
   - Manager receives email with broker details and approval/rejection links

## API Changes

- `CreateBrokerRequest` now includes `ManagerEmail` (required)
- Manager validation checks company association
- Approval emails go to the broker's manager

## Frontend Changes

- Added "Manager Email *" field (required)
- Field appears after Company Name field
- Help text: "Enter your manager's email address for approval"

## Deployment Steps

1. **Run Database Migration:**
   ```bash
   psql -h YOUR_RDS_HOST -U postgres -d insuranceloom -f InsuranceLoom.Api/Data/Migrations/003_AddManagerCompanyAndInitialManager.sql
   ```

2. **Deploy API Code:**
   ```bash
   cd ~/Insurance_loom
   git pull origin main
   cd InsuranceLoom.Api
   ./deploy.sh
   ```

3. **Verify Manager:**
   ```sql
   SELECT m.*, c.name as company_name 
   FROM managers m 
   LEFT JOIN companies c ON m.company_id = c.id 
   WHERE m.email = 'ico@astutetech.co.za';
   ```

## Future: Managers Page

A managers management page will be created later to:
- Add/edit/delete managers
- Associate managers with companies
- Manage manager permissions

