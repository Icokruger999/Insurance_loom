# Setting Up AWS RDS with Your Credentials

This guide will help you set up AWS RDS using the provided AWS credentials securely.

## ⚠️ Security Note

**NEVER commit AWS credentials to Git!** We'll use environment variables or AWS CLI configuration.

## Step 1: Install AWS CLI (if not already installed)

Download and install AWS CLI from: https://aws.amazon.com/cli/

Verify installation:
```powershell
aws --version
```

## Step 2: Configure AWS Credentials

### Option A: Environment Variables (Recommended for this session)

Open PowerShell and run:

```powershell
$env:AWS_ACCESS_KEY_ID="YOUR_AWS_ACCESS_KEY_ID"
$env:AWS_SECRET_ACCESS_KEY="YOUR_AWS_SECRET_ACCESS_KEY"
$env:AWS_DEFAULT_REGION="af-south-1"
```

**Note**: These will only last for the current PowerShell session.

### Option B: AWS Configure (Permanent)

Run:
```powershell
aws configure
```

Enter when prompted:
- AWS Access Key ID: `YOUR_AWS_ACCESS_KEY_ID`
- AWS Secret Access Key: `YOUR_AWS_SECRET_ACCESS_KEY`
- Default region name: `af-south-1`
- Default output format: `json`

## Step 3: Verify Credentials

Test your credentials:
```powershell
aws sts get-caller-identity
```

You should see your AWS account details.

## Step 4: Run the Setup Script

Navigate to the API directory:
```powershell
cd InsuranceLoom.Api
```

Run the setup script:
```powershell
.\setup-aws-rds.ps1
```

The script will:
1. ✅ Check AWS CLI and credentials
2. ✅ Create security group for RDS
3. ✅ Create RDS PostgreSQL instance (db.t3.micro, free tier)
4. ✅ Configure network access
5. ✅ Provide next steps

**The script will prompt you for:**
- Database master password (create a strong password)

## Step 5: Wait for RDS Instance

The RDS instance creation takes 5-10 minutes. 

Check status:
```powershell
aws rds describe-db-instances --db-instance-identifier insuranceloom-db --region af-south-1 --query 'DBInstances[0].DBInstanceStatus' --output text
```

Wait until status is `available`.

## Step 6: Get RDS Endpoint

Once the instance is available, get the endpoint:

**Option A: Use the helper script**
```powershell
.\get-rds-endpoint.ps1
```

**Option B: Manual command**
```powershell
aws rds describe-db-instances --db-instance-identifier insuranceloom-db --region af-south-1 --query 'DBInstances[0].Endpoint.Address' --output text
```

You'll get something like: `insuranceloom-db.xxxxx.af-south-1.rds.amazonaws.com`

## Step 7: Update appsettings.json

Open `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_ENDPOINT_HERE;Port=5432;Database=insuranceloom;Username=postgres;Password=YOUR_PASSWORD_HERE;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
  }
}
```

Replace:
- `YOUR_ENDPOINT_HERE` with the endpoint from Step 6
- `YOUR_PASSWORD_HERE` with the password you created in Step 4

**Example:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=insuranceloom-db.abc123xyz.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=MySecureP@ssw0rd!;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
  }
}
```

## Step 8: Run Database Migration

See `Data/Migrations/README.md` for detailed instructions.

**Quick method using pgAdmin:**
1. Download pgAdmin: https://www.pgadmin.org/download/
2. Add server with your RDS endpoint
3. Connect to `insuranceloom` database
4. Run `Data/Migrations/001_InitialSchema.sql`

## Step 9: Test Connection

Run your application:
```powershell
dotnet run
```

Check for connection errors. If successful, Swagger UI will be available at `https://localhost:5001/swagger`

## Manual Setup (Alternative)

If you prefer to set up manually through AWS Console:

1. Go to AWS Console → RDS → Create Database
2. Follow the guide in `AWS_RDS_SETUP.md`
3. Use your existing AWS credentials to log in

## Troubleshooting

### "Access Denied" Error

- Verify credentials are correct
- Check IAM user has RDS permissions
- Ensure region is `af-south-1` (Cape Town)

### "Security Group" Error

- The script creates a security group automatically
- If it fails, create manually in AWS Console
- Allow port 5432 from your IP address

### "Instance Already Exists"

- The instance `insuranceloom-db` might already exist
- Check AWS Console → RDS
- Use existing instance or delete and recreate

### Connection Timeout

- Check security group allows your IP on port 5432
- Verify RDS instance status is "available"
- Ensure public accessibility is enabled

## Security Best Practices

1. ✅ **Don't commit credentials to Git**
   - Use environment variables
   - Use AWS credentials file (not in repo)
   - Use AWS IAM roles on EC2

2. ✅ **Use strong database password**
   - Minimum 12 characters
   - Mix of uppercase, lowercase, numbers, special chars

3. ✅ **Restrict security group**
   - Only allow your IP address
   - Don't use `0.0.0.0/0` (all IPs)

4. ✅ **Enable SSL in connection string**
   - Already included: `SSL Mode=Prefer;`

5. ✅ **Regular backups**
   - Free tier includes 7 days
   - Test restore procedures

## Cost Monitoring

The setup uses AWS Free Tier:
- ✅ db.t3.micro instance (750 hours/month)
- ✅ 20 GB storage
- ✅ 20 GB backup storage
- ✅ 100 GB data transfer/month

**Free tier valid for 12 months from AWS account creation.**

Monitor costs:
- AWS Console → Billing Dashboard
- Set up billing alerts

## Next Steps

After successful setup:
1. ✅ RDS instance created and available
2. ✅ Connection string updated
3. ✅ Migration script executed
4. ✅ Application tested

Your database is ready to use! 🎉

