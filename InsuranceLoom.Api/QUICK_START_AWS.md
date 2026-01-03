# Quick Start - AWS RDS Setup with Your Credentials

## 🚀 Fastest Way to Set Up RDS

### Step 1: Set AWS Credentials (PowerShell)

Open PowerShell and run:

```powershell
$env:AWS_ACCESS_KEY_ID="YOUR_AWS_ACCESS_KEY_ID"
$env:AWS_SECRET_ACCESS_KEY="YOUR_AWS_SECRET_ACCESS_KEY"
$env:AWS_DEFAULT_REGION="af-south-1"
```

### Step 2: Verify Credentials

```powershell
aws sts get-caller-identity
```

Should show your AWS account info.

### Step 3: Run Setup Script

```powershell
cd InsuranceLoom.Api
.\setup-aws-rds.ps1
```

**The script will:**
- ✅ Create security group
- ✅ Create RDS PostgreSQL instance (free tier)
- ✅ Configure network access
- ✅ Prompt for database password

**Wait 5-10 minutes for RDS to be created.**

### Step 4: Get Endpoint (Once Available)

```powershell
.\get-rds-endpoint.ps1
```

This shows your connection details.

### Step 5: Update appsettings.json

Update the connection string with:
- Endpoint from Step 4
- Password you created in Step 3

### Step 6: Run Migration

Use pgAdmin or psql to run `Data/Migrations/001_InitialSchema.sql`

### Step 7: Test

```powershell
dotnet run
```

Visit `https://localhost:5001/swagger`

## 📋 Full Instructions

See `SETUP_WITH_AWS_CREDENTIALS.md` for detailed guide.

## 🔒 Security

- Credentials are only in environment variables (not saved)
- Don't commit credentials to Git
- Use strong database password

## ✅ What Gets Created

- RDS PostgreSQL instance: `insuranceloom-db`
- Security group: `insuranceloom-rds-sg`
- Database: `insuranceloom`
- Username: `postgres`
- Region: `af-south-1` (Cape Town)

All using AWS Free Tier! 🎉

