# Database Migration Instructions

## Running the Migration Script

After setting up your AWS RDS PostgreSQL instance, you need to run the migration script to create all tables and seed initial data.

## Method 1: Using pgAdmin (Recommended for Beginners)

1. **Download pgAdmin**: https://www.pgadmin.org/download/
2. **Install and open pgAdmin**
3. **Add Server**:
   - Right-click "Servers" → "Create" → "Server"
   - **General Tab**:
     - Name: `Insurance Loom RDS`
   - **Connection Tab**:
     - Host: Your RDS endpoint (e.g., `insuranceloom-db.xxxxx.af-south-1.rds.amazonaws.com`)
     - Port: `5432`
     - Database: `insuranceloom`
     - Username: `postgres`
     - Password: Your master password
   - Click **Save**
4. **Connect to Server**:
   - Expand "Insurance Loom RDS" → "Databases" → "insuranceloom"
5. **Run Migration Script**:
   - Right-click on `insuranceloom` database → "Query Tool"
   - Click "Open File" icon
   - Navigate to `Data/Migrations/001_InitialSchema.sql`
   - Click "Execute" (F5) or click the play button
   - Wait for "Query returned successfully" message

## Method 2: Using psql Command Line

### Windows (with PostgreSQL installed)

```powershell
# Navigate to project directory
cd C:\insurance_loom\InsuranceLoom.Api

# Run migration
psql -h YOUR_RDS_ENDPOINT -p 5432 -U postgres -d insuranceloom -f Data\Migrations\001_InitialSchema.sql
```

### Linux/Mac

```bash
# Navigate to project directory
cd InsuranceLoom.Api

# Run migration
psql -h YOUR_RDS_ENDPOINT -p 5432 -U postgres -d insuranceloom -f Data/Migrations/001_InitialSchema.sql
```

**Replace `YOUR_RDS_ENDPOINT` with your actual RDS endpoint**

## Method 3: Using AWS CloudShell

1. **Open AWS CloudShell** (icon in top right of AWS Console)
2. **Install PostgreSQL client**:
   ```bash
   sudo yum install postgresql15 -y
   ```
3. **Download migration script** (or upload it):
   ```bash
   # If you have the file locally, you can upload it via CloudShell UI
   # Or clone your repo if it's in GitHub
   ```
4. **Run migration**:
   ```bash
   psql -h YOUR_RDS_ENDPOINT -p 5432 -U postgres -d insuranceloom -f 001_InitialSchema.sql
   ```

## Method 4: Using DBeaver (Free Database Tool)

1. **Download DBeaver**: https://dbeaver.io/download/
2. **Create New Connection**:
   - Select "PostgreSQL"
   - **Main Tab**:
     - Host: Your RDS endpoint
     - Port: `5432`
     - Database: `insuranceloom`
     - Username: `postgres`
     - Password: Your master password
   - Click "Test Connection" → "Finish"
3. **Run Script**:
   - Right-click on `insuranceloom` → "SQL Editor" → "New SQL Script"
   - Open `001_InitialSchema.sql`
   - Click "Execute SQL Script" (Ctrl+Enter)

## Verification

After running the migration, verify tables were created:

```sql
-- Connect to database and run:
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;
```

You should see these tables:
- brokers
- claims
- csv_import_errors
- csv_imports
- debit_orders
- document_types
- documents
- managers
- payment
- policy_approval_history
- policy_approvals
- policy_holders
- policies
- service_document_requirements
- service_types
- users

## Seed Data Verification

Verify seed data was inserted:

```sql
-- Check service types
SELECT * FROM service_types;

-- Should return:
-- FUNERAL, LIFE, HEALTH, DISABILITY, INCOME

-- Check document types
SELECT document_code, document_name FROM document_types;

-- Should return multiple document types including:
-- ID_DOCUMENT, PROOF_OF_ADDRESS, MEDICAL_CERTIFICATE, etc.
```

## Troubleshooting

### Error: "relation already exists"
- Tables may already exist. Drop and recreate:
  ```sql
  DROP SCHEMA public CASCADE;
  CREATE SCHEMA public;
  ```
  Then re-run the migration script.

### Error: "permission denied"
- Ensure you're using the master user (`postgres`)
- Check user has CREATE privileges

### Error: "connection refused"
- Verify RDS endpoint is correct
- Check security group allows your IP on port 5432
- Ensure RDS instance status is "Available"

### Error: "database does not exist"
- The database `insuranceloom` should be created automatically when you create the RDS instance
- If not, create it manually:
  ```sql
  CREATE DATABASE insuranceloom;
  ```

## Next Steps

After successful migration:
1. ✅ All tables created
2. ✅ Indexes created
3. ✅ Seed data inserted
4. ✅ Ready to use with API

Test the connection from your application:
```bash
cd InsuranceLoom.Api
dotnet run
```

Check for any database connection errors in the console.

