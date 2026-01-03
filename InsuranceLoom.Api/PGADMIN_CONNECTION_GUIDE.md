# Connecting to AWS RDS PostgreSQL using pgAdmin

## Prerequisites

1. **Download and Install pgAdmin**
   - Download from: https://www.pgadmin.org/download/
   - Install pgAdmin 4 (latest version)

2. **RDS Instance Must Be Available**
   - Status should be "available" (not "creating")
   - Check status using: `.\check-rds-status.ps1`

## Connection Details

**From RDS_CREDENTIALS.txt:**
- **Host/Address**: [Get from check-rds-status.ps1 or AWS Console]
- **Port**: `5432`
- **Database**: `insuranceloom`
- **Username**: `postgres`
- **Password**: `YOUR_DATABASE_PASSWORD`

## Step-by-Step Connection Instructions

### Step 1: Open pgAdmin

Launch pgAdmin 4 from your Start menu or desktop shortcut.

### Step 2: Add New Server

1. In the left panel, right-click on **"Servers"**
2. Select **"Create"** → **"Server..."**

### Step 3: General Tab

1. **Name**: `Insurance Loom RDS` (or any name you prefer)
2. Click **"Connection"** tab

### Step 4: Connection Tab

Fill in the following details:

**Basic Settings:**
- **Host name/address**: Enter your RDS endpoint
  - Format: `insuranceloom-db.xxxxx.af-south-1.rds.amazonaws.com`
  - Get this from: `.\check-rds-status.ps1` or AWS Console → RDS → Your instance → Endpoint

- **Port**: `5432`

- **Maintenance database**: `insuranceloom`

- **Username**: `postgres`

- **Password**: `YOUR_DATABASE_PASSWORD`
  - ⚠️ **Important**: Check "Save password" if you want pgAdmin to remember it

**Advanced Settings:**
- **Service**: Leave empty

**SSL Tab (Recommended):**
- **SSL mode**: Select **"Prefer"** or **"Require"**
  - This ensures secure connection to AWS RDS

### Step 5: Save and Connect

1. Click **"Save"** button
2. pgAdmin will attempt to connect
3. If successful, you'll see the server in the left panel with a green icon
4. Expand the server to see databases

### Step 6: Verify Connection

1. Expand **"Insurance Loom RDS"**
2. Expand **"Databases"**
3. You should see the **"insuranceloom"** database
4. Expand it to see **"Schemas"** → **"public"**

## Running the Migration Script

Once connected, you can run the database migration script:

### Method 1: Using Query Tool

1. Right-click on **"insuranceloom"** database
2. Select **"Query Tool"**
3. Click the **"Open File"** icon (folder icon) in the toolbar
4. Navigate to: `InsuranceLoom.Api\Data\Migrations\001_InitialSchema.sql`
5. Click **"Execute"** (F5) or click the play button (▶)
6. Wait for "Query returned successfully" message

### Method 2: Using PSQL Tool

1. Right-click on **"insuranceloom"** database
2. Select **"PSQL Tool"**
3. Copy and paste the contents of `001_InitialSchema.sql`
4. Execute the script

## Troubleshooting

### Connection Timeout

**Problem**: Cannot connect to server, connection timed out

**Solutions**:
1. Check RDS instance status is "available" (not "creating")
2. Verify security group allows your IP on port 5432
3. Check your firewall/antivirus isn't blocking port 5432
4. Verify the endpoint address is correct

### Authentication Failed

**Problem**: Password authentication failed

**Solutions**:
1. Double-check username is `postgres` (case-sensitive)
2. Verify password is correct: `1bHiVZ0odtB?&+S$`
3. Check for extra spaces or typos
4. Try copying password from RDS_CREDENTIALS.txt

### SSL Connection Error

**Problem**: SSL connection error

**Solutions**:
1. Set SSL mode to **"Prefer"** or **"Allow"** (not "Require")
2. If using "Require", you may need to download AWS RDS CA certificate
3. For development, "Prefer" should work fine

### Host Not Found

**Problem**: Could not resolve hostname

**Solutions**:
1. Verify the endpoint is correct (get from AWS Console)
2. Check your internet connection
3. Try pinging the endpoint: `ping insuranceloom-db.xxxxx.af-south-1.rds.amazonaws.com`

### Security Group Issues

**Problem**: Connection refused or timeout

**Solutions**:
1. Check security group: `sg-0839351aef0ca19f8`
2. Verify inbound rule allows port 5432 from your IP
3. Get your current IP: Visit https://api.ipify.org
4. Update security group in AWS Console if your IP changed

## Getting Your Current IP for Security Group

If you need to update the security group with your current IP:

1. Visit: https://api.ipify.org (shows your public IP)
2. Go to AWS Console → EC2 → Security Groups
3. Find: `insuranceloom-rds-sg`
4. Edit inbound rules
5. Add rule: PostgreSQL, Port 5432, Source: Your IP/32

## Useful pgAdmin Features

### View Database Structure

- Expand database → Schemas → public → Tables
- See all tables created by migration script

### Run Queries

- Right-click database → Query Tool
- Write and execute SQL queries
- View results in data grid

### Backup/Restore

- Right-click database → Backup/Restore
- Export database or import backups

### View Data

- Expand Tables → Right-click table → View/Edit Data → All Rows
- View table data in a spreadsheet-like interface

## Next Steps After Connection

1. ✅ Connect to RDS using pgAdmin
2. ✅ Run migration script: `001_InitialSchema.sql`
3. ✅ Verify tables were created (check Schemas → public → Tables)
4. ✅ Update `appsettings.json` with connection string
5. ✅ Test application connection: `dotnet run`

## Security Notes

- ⚠️ The security group currently allows connections from anywhere (0.0.0.0/0)
- 🔒 For production, restrict to specific IP addresses
- 🔒 Consider using SSL mode "Require" for production
- 🔒 Never commit passwords to Git
- 🔒 Delete RDS_CREDENTIALS.txt after saving credentials securely

