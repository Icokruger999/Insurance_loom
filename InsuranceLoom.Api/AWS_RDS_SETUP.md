# AWS RDS PostgreSQL Setup Guide (Free Tier)

This guide will help you set up a PostgreSQL database on AWS RDS using the free tier.

## AWS RDS Free Tier Eligibility

- **Duration**: 12 months from AWS account creation
- **Instance Type**: db.t3.micro or db.t2.micro
- **Storage**: 20 GB General Purpose (SSD) storage
- **Database Engine**: PostgreSQL (up to version 15.x)
- **Multi-AZ**: Not available on free tier (single availability zone only)
- **Backups**: 20 GB of backup storage

## Prerequisites

1. AWS Account (if you don't have one, sign up at https://aws.amazon.com)
2. AWS Console access
3. Basic understanding of AWS services

## Step-by-Step Setup

### Step 1: Navigate to RDS Console

1. Log in to AWS Console: https://console.aws.amazon.com
2. Search for "RDS" in the services search bar
3. Click on "RDS" to open the RDS Dashboard

### Step 2: Create Database

1. Click the **"Create database"** button
2. Choose **"Standard create"** (not Easy create)
3. Select **"PostgreSQL"** as the database engine
4. For **Version**, select **PostgreSQL 15.x** (latest stable version)

### Step 3: Templates

1. Select **"Free tier"** template
   - This automatically selects db.t3.micro instance class
   - Sets appropriate storage and backup settings

### Step 4: Settings

Configure the following:

**DB instance identifier**: `insuranceloom-db`
- This is your database instance name

**Master username**: `postgres` (or your preferred username)
- **Important**: Remember this username!

**Master password**: Create a strong password
- Minimum 8 characters
- Must contain uppercase, lowercase, numbers, and special characters
- **Important**: Save this password securely!

**Confirm password**: Re-enter the password

### Step 5: Instance Configuration

**DB instance class**: `db.t3.micro` (should be pre-selected for free tier)
- 2 vCPUs
- 1 GB RAM
- This is sufficient for development/testing

### Step 6: Storage

**Storage type**: `General Purpose SSD (gp3)`
- Free tier includes 20 GB

**Allocated storage**: `20` GB
- This is the free tier limit

**Storage autoscaling**: **Uncheck** this option
- Free tier doesn't support autoscaling
- You can manually increase later if needed (will incur charges)

### Step 7: Connectivity

**Virtual Private Cloud (VPC)**: Use default VPC
- Or create a new VPC if you prefer

**Subnet group**: Use default subnet group

**Public access**: **Yes** (for easier connection)
- ⚠️ **Security Note**: For production, set to "No" and use VPN or bastion host

**VPC security group**: Create new security group
- Name: `insuranceloom-db-sg`
- We'll configure rules in the next step

**Availability Zone**: Leave as default (No preference)

**DB port**: `5432` (default PostgreSQL port)

### Step 8: Database Authentication

**Database authentication options**: `Password authentication`
- This is the simplest option for free tier

### Step 9: Additional Configuration

**Initial database name**: `insuranceloom`
- This creates a database with this name automatically

**DB parameter group**: Use default

**Backup retention period**: `7 days` (free tier allows up to 7 days)

**Backup window**: Leave as default (or choose a time when database is less used)

**Enable encryption**: **Uncheck** (free tier doesn't support encryption)
- For production, enable encryption

**Enable Enhanced monitoring**: **Uncheck** (not available on free tier)

**Enable Performance Insights**: **Uncheck** (not available on free tier)

**Enable deletion protection**: **Uncheck** (for development)
- ⚠️ For production, enable this to prevent accidental deletion

### Step 10: Review and Create

1. Review all settings
2. Click **"Create database"**
3. Wait 5-10 minutes for the database to be created

### Step 11: Configure Security Group

After the database is created:

1. Go to **RDS Dashboard** → Click on your database instance
2. Scroll down to **"Connectivity & security"** section
3. Click on the **Security group** link (e.g., `insuranceloom-db-sg`)

4. In the Security Group page:
   - Click **"Edit inbound rules"**
   - Click **"Add rule"**
   - **Type**: PostgreSQL
   - **Protocol**: TCP
   - **Port**: 5432
   - **Source**: 
     - For development: `My IP` (your current IP address)
     - For testing from multiple locations: `0.0.0.0/0` (⚠️ Not recommended for production)
   - Click **"Save rules"**

### Step 12: Get Connection Details

1. Go back to **RDS Dashboard**
2. Click on your database instance: `insuranceloom-db`
3. In **"Connectivity & security"** section, note:
   - **Endpoint**: e.g., `insuranceloom-db.xxxxxxxxx.af-south-1.rds.amazonaws.com`
   - **Port**: `5432`
   - **Database name**: `insuranceloom`
   - **Username**: `postgres` (or what you set)

## Step 13: Update Application Configuration

Update `appsettings.json` with your RDS connection details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_ENDPOINT_HERE;Port=5432;Database=insuranceloom;Username=postgres;Password=YOUR_PASSWORD_HERE;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;"
  }
}
```

Replace:
- `YOUR_ENDPOINT_HERE` with your RDS endpoint
- `YOUR_PASSWORD_HERE` with your master password

Example:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=insuranceloom-db.abc123xyz.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=MySecureP@ssw0rd!;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;"
  }
}
```

## Step 14: Run Database Migration

### Option A: Using psql (Command Line)

If you have PostgreSQL client installed:

```bash
# Connect to RDS instance
psql -h YOUR_ENDPOINT_HERE -p 5432 -U postgres -d insuranceloom

# Enter password when prompted
# Then run the migration script
\i Data/Migrations/001_InitialSchema.sql
```

### Option B: Using pgAdmin

1. Download and install pgAdmin: https://www.pgadmin.org/download/
2. Add new server:
   - **Name**: Insurance Loom RDS
   - **Host**: Your RDS endpoint
   - **Port**: 5432
   - **Database**: insuranceloom
   - **Username**: postgres
   - **Password**: Your master password
3. Connect to the server
4. Right-click on `insuranceloom` database → **Query Tool**
5. Open `Data/Migrations/001_InitialSchema.sql`
6. Execute the script

### Option C: Using AWS CloudShell

1. In AWS Console, click the CloudShell icon (top right)
2. Install PostgreSQL client:
   ```bash
   sudo yum install postgresql15 -y
   ```
3. Connect and run migration:
   ```bash
   psql -h YOUR_ENDPOINT_HERE -p 5432 -U postgres -d insuranceloom -f Data/Migrations/001_InitialSchema.sql
   ```

## Step 15: Test Connection

Test the connection from your application:

```bash
cd InsuranceLoom.Api
dotnet run
```

Check the console for any connection errors. If successful, you should see:
- Swagger UI available
- No database connection errors

## Cost Monitoring

### Free Tier Limits

- **Instance**: db.t3.micro (750 hours/month)
- **Storage**: 20 GB General Purpose SSD
- **Backups**: 20 GB backup storage
- **Data Transfer**: 100 GB outbound per month

### What Costs Money (After Free Tier)

- **Storage beyond 20 GB**: ~$0.115 per GB/month
- **Backup storage beyond 20 GB**: ~$0.095 per GB/month
- **Data transfer out**: First 100 GB free, then ~$0.09 per GB
- **Instance hours beyond 750**: ~$0.017 per hour

### Monitor Costs

1. Go to **AWS Cost Explorer**
2. Set up billing alerts:
   - Go to **Billing Dashboard** → **Preferences**
   - Enable **"Receive Billing Alerts"**
   - Set up CloudWatch alarms for spending thresholds

## Security Best Practices

1. **Change Default Port** (Optional):
   - Use a non-standard port for additional security
   - Update security group and connection string

2. **Use Strong Passwords**:
   - Minimum 12 characters
   - Mix of uppercase, lowercase, numbers, special characters

3. **Restrict Security Group**:
   - Only allow your IP address or specific IP ranges
   - Never use `0.0.0.0/0` in production

4. **Enable SSL/TLS** (Recommended):
   - PostgreSQL RDS supports SSL connections
   - Update connection string: `SSL Mode=Require;`

5. **Regular Backups**:
   - Free tier includes 7 days of backups
   - Test restore procedures

6. **Monitor Access**:
   - Enable CloudTrail for audit logging
   - Review security group rules regularly

## Troubleshooting

### Connection Timeout

- Check security group rules (port 5432 must be open)
- Verify your IP address hasn't changed
- Check RDS instance status (must be "Available")

### Authentication Failed

- Verify username and password
- Check if database name is correct
- Ensure database exists

### Out of Free Tier

- Check AWS account age (must be < 12 months)
- Verify you're using db.t3.micro instance
- Check storage is ≤ 20 GB

### Performance Issues

- db.t3.micro has limited resources (1 GB RAM)
- Consider upgrading for production (will incur costs)
- Optimize queries and use indexes

## Next Steps

1. ✅ Database created and configured
2. ✅ Security group configured
3. ✅ Connection string updated
4. ✅ Migration script executed
5. ✅ Test connection successful

Your PostgreSQL database is now ready to use with the Insurance Loom API!

## Support

If you encounter issues:
- Check AWS RDS documentation
- Review CloudWatch logs
- Verify security group settings
- Test connection from different network

