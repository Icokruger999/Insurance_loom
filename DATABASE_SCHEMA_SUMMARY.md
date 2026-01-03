# Database Schema Summary - Policy Holder Application Fields

## Overview
All form fields from the application are now stored in database tables. The system automatically runs SQL migrations on startup.

## Tables and Fields

### 1. `policy_holders` Table
All customer/policy holder information is stored here:

**Personal Information:**
- `first_name` - First Name
- `last_name` - Last Name  
- `middle_name` - Middle Name (NEW)
- `id_number` - ID Number
- `phone` - Phone Number
- `email` (stored in `users` table, linked via `user_id`)
- `address` - Residence Address
- `date_of_birth` - Date of Birth
- `birthplace` - Birthplace (NEW)
- `sex` - Sex/Gender (NEW)
- `civil_status` - Civil Status (NEW)
- `occupation` - Occupation (NEW)

**Financial Information:**
- `monthly_income` - Monthly Income (NEW)
- `monthly_expenses` - Monthly Expenses (NEW)

**Employment/Employer Information:**
- `employment_type` - Employment Type (Employee/Self Employed) (NEW)
- `income_tax_number` - Income Tax Number (NEW)
- `employment_start_date` - Employment Start Date (NEW)
- `employment_end_date` - Employment End Date (NEW)
- `agency_name` - Agency/Employer Name (NEW)
- `agency_contact_no` - Agency Contact Number (NEW)
- `agency_address` - Agency/Employer Address (NEW)
- `agency_email` - Agency/Employer Email (NEW)
- `agency_signatory` - Authorized Signatory Name (NEW)

**System Fields:**
- `id` - Primary Key (UUID)
- `user_id` - Foreign Key to `users` table
- `policy_number` - Unique Policy Number
- `is_active` - Active Status
- `created_at` - Creation Timestamp
- `updated_at` - Last Update Timestamp

### 2. `beneficiaries` Table (NEW)
Stores beneficiary information for policy holders:

- `id` - Primary Key (UUID)
- `policy_holder_id` - Foreign Key to `policy_holders` table
- `policy_id` - Optional Foreign Key to `policies` table
- `full_name` - Full Name
- `date_of_birth` - Date of Birth
- `age` - Age
- `mobile` - Mobile Number
- `email` - Email Address
- `relationship` - Relationship to Policy Holder
- `type` - Type (Revocable/Irrevocable)
- `created_at` - Creation Timestamp
- `updated_at` - Last Update Timestamp

### 3. `policies` Table
Stores policy information (already exists):
- Policy details, coverage amounts, premium amounts, etc.

### 4. `users` Table
Stores user authentication information:
- `email` - Email Address (used for login)
- `password_hash` - Hashed Password
- `user_type` - User Type (PolicyHolder, Broker, Manager)

## SQL Migrations

The following migrations have been created and run automatically on application startup:

1. **006_AddPolicyHolderAdditionalFields.sql**
   - Adds all new fields to `policy_holders` table
   - Includes comments for documentation

2. **007_CreateBeneficiariesTable.sql**
   - Creates the `beneficiaries` table
   - Sets up foreign key relationships
   - Creates indexes for performance

## Update Functionality

### API Endpoint
**PUT `/api/policyholder/{id}`**
- Updates policy holder information
- Updates beneficiaries
- Requires authentication (Broker or Manager role)

### Usage Example
```bash
PUT /api/policyholder/{policyHolderId}
Content-Type: multipart/form-data
Authorization: Bearer {token}

{
  "firstName": "John",
  "lastName": "Doe",
  "middleName": "Michael",
  "email": "john.doe@example.com",
  "phone": "+27123456789",
  "incomeTaxNumber": "1234567890",
  "employmentType": "Employee",
  "beneficiaries": [
    {
      "fullName": "Jane Doe",
      "dateOfBirth": "1990-01-01",
      "relationship": "Spouse",
      "type": "Revocable"
    }
  ]
}
```

## Automatic Migration Execution

Migrations run automatically when the application starts via `MigrationRunner.RunMigrationsAsync()` in `Program.cs`. The system:

1. Runs Entity Framework Core migrations
2. Executes all SQL files in `Data/Migrations/` directory in alphabetical order
3. Continues even if individual migrations fail (logs errors)
4. Uses `IF NOT EXISTS` and `ADD COLUMN IF NOT EXISTS` to avoid errors on re-runs

## Field Mapping Summary

| Form Field | Database Column | Table | Type |
|------------|----------------|-------|------|
| First Name | `first_name` | `policy_holders` | VARCHAR |
| Last Name | `last_name` | `policy_holders` | VARCHAR |
| Middle Name | `middle_name` | `policy_holders` | VARCHAR |
| ID Number | `id_number` | `policy_holders` | VARCHAR |
| Email | `email` | `users` | VARCHAR |
| Phone | `phone` | `policy_holders` | VARCHAR |
| Address | `address` | `policy_holders` | TEXT |
| Date of Birth | `date_of_birth` | `policy_holders` | DATE |
| Birthplace | `birthplace` | `policy_holders` | VARCHAR |
| Sex | `sex` | `policy_holders` | VARCHAR(20) |
| Civil Status | `civil_status` | `policy_holders` | VARCHAR(50) |
| Occupation | `occupation` | `policy_holders` | VARCHAR |
| Monthly Income | `monthly_income` | `policy_holders` | DECIMAL(18,2) |
| Monthly Expenses | `monthly_expenses` | `policy_holders` | DECIMAL(18,2) |
| Employment Type | `employment_type` | `policy_holders` | VARCHAR(50) |
| Income Tax Number | `income_tax_number` | `policy_holders` | VARCHAR(50) |
| Employment Start Date | `employment_start_date` | `policy_holders` | DATE |
| Employment End Date | `employment_end_date` | `policy_holders` | DATE |
| Agency Name | `agency_name` | `policy_holders` | VARCHAR |
| Agency Contact | `agency_contact_no` | `policy_holders` | VARCHAR |
| Agency Address | `agency_address` | `policy_holders` | TEXT |
| Agency Email | `agency_email` | `policy_holders` | VARCHAR |
| Agency Signatory | `agency_signatory` | `policy_holders` | VARCHAR |
| Beneficiary Name | `full_name` | `beneficiaries` | VARCHAR |
| Beneficiary DOB | `date_of_birth` | `beneficiaries` | DATE |
| Beneficiary Relationship | `relationship` | `beneficiaries` | VARCHAR |
| Beneficiary Type | `type` | `beneficiaries` | VARCHAR |

## Notes

- All migrations use `IF NOT EXISTS` clauses to allow safe re-execution
- The system automatically tracks which migrations have been applied
- New fields are nullable to allow updates to existing records
- Foreign key constraints ensure data integrity
- Indexes are created on foreign keys for query performance

