# Installing PostgreSQL on Windows

## Option 1: Download and Install PostgreSQL (Recommended)

### Step 1: Download PostgreSQL

1. Go to: https://www.postgresql.org/download/windows/
2. Click **"Download the installer"**
3. Or use direct link: https://www.enterprisedb.com/downloads/postgres-postgresql-downloads
4. Download **PostgreSQL 15.x** (latest version)

### Step 2: Run Installer

1. Run the downloaded `.exe` file
2. Follow the installation wizard:

   **Installation Directory:**
   - Default: `C:\Program Files\PostgreSQL\15` (or latest version)
   - Click **Next**

   **Select Components:**
   - ✅ PostgreSQL Server
   - ✅ pgAdmin 4 (GUI tool - recommended!)
   - ✅ Stack Builder
   - ✅ Command Line Tools
   - Click **Next**

   **Data Directory:**
   - Default: `C:\Program Files\PostgreSQL\15\data`
   - Click **Next**

   **Password:**
   - Set a password for the `postgres` superuser
   - **Important**: Remember this password!
   - Click **Next**

   **Port:**
   - Default: `5432`
   - Keep default (unless you have conflicts)
   - Click **Next**

   **Advanced Options:**
   - Locale: Default
   - Click **Next**

   **Ready to Install:**
   - Review settings
   - Click **Next**

   **Installing:**
   - Wait for installation to complete
   - Click **Finish**

### Step 3: Verify Installation

Open PowerShell and run:
```powershell
psql --version
```

You should see: `psql (PostgreSQL) 15.x`

## Option 2: Install via Chocolatey (If you have Chocolatey)

```powershell
choco install postgresql15
```

## Option 3: Install via Winget (Windows Package Manager)

```powershell
winget install PostgreSQL.PostgreSQL
```

## After Installation

### Add PostgreSQL to PATH (if not automatically added)

1. Open **System Properties** → **Environment Variables**
2. Under **System Variables**, find **Path**
3. Click **Edit**
4. Add: `C:\Program Files\PostgreSQL\15\bin`
5. Click **OK** on all dialogs
6. Restart PowerShell

### Verify Installation

```powershell
psql --version
```

## Connecting to AWS RDS from Command Line

Once PostgreSQL is installed, you can connect to your RDS instance:

```powershell
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -p 5432 -U postgres -d insuranceloom
```

When prompted, enter password: `YOUR_DATABASE_PASSWORD`

## Running Migration Script from Command Line

```powershell
cd InsuranceLoom.Api
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -p 5432 -U postgres -d insuranceloom -f Data\Migrations\001_InitialSchema.sql
```

## Using pgAdmin (Included with PostgreSQL)

pgAdmin 4 is installed with PostgreSQL and provides a GUI:

1. Open **pgAdmin 4** from Start menu
2. Set master password (first time only)
3. Add server using connection details from `RDS_CREDENTIALS.txt`
4. Connect and run migration script

## Troubleshooting

### "psql is not recognized"

**Solution**: Add PostgreSQL bin directory to PATH (see above)

### Connection Refused

**Solution**: 
- Check RDS instance status is "available"
- Verify security group allows your IP
- Check endpoint is correct

### Password Authentication Failed

**Solution**:
- Verify password: `1bHiVZ0odtB?&+S$`
- Check username is `postgres`
- Try using `PGPASSWORD` environment variable:
  ```powershell
  $env:PGPASSWORD="1bHiVZ0odtB?&+S$"
  psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -p 5432 -U postgres -d insuranceloom
  ```

## Quick Connection Script

Create a file `connect-rds.ps1`:

```powershell
$env:PGPASSWORD="1bHiVZ0odtB?&+S$"
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -p 5432 -U postgres -d insuranceloom
```

Run it: `.\connect-rds.ps1`

