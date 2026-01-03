# AWS RDS Setup Script for Insurance Loom
# This script helps set up RDS PostgreSQL instance in AWS Cape Town (af-south-1)

# IMPORTANT: Set your AWS credentials as environment variables first
# DO NOT hardcode credentials in this script

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Insurance Loom - AWS RDS Setup" -ForegroundColor Cyan
Write-Host "Region: Cape Town (af-south-1)" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if AWS CLI is installed
try {
    $awsVersion = aws --version 2>&1
    Write-Host "✓ AWS CLI found: $awsVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ AWS CLI not found. Please install AWS CLI first:" -ForegroundColor Red
    Write-Host "  https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
}

# Check if credentials are configured
Write-Host ""
Write-Host "Checking AWS credentials..." -ForegroundColor Yellow
try {
    $identity = aws sts get-caller-identity 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ AWS credentials are configured" -ForegroundColor Green
        Write-Host "  $identity" -ForegroundColor Gray
    } else {
        Write-Host "✗ AWS credentials not configured" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please configure AWS credentials using one of these methods:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Method 1: Environment Variables (Recommended for this session)" -ForegroundColor Cyan
        Write-Host '  $env:AWS_ACCESS_KEY_ID="YOUR_AWS_ACCESS_KEY_ID"' -ForegroundColor White
        Write-Host '  $env:AWS_SECRET_ACCESS_KEY="YOUR_AWS_SECRET_ACCESS_KEY"' -ForegroundColor White
        Write-Host '  $env:AWS_DEFAULT_REGION="af-south-1"' -ForegroundColor White
        Write-Host ""
        Write-Host "Method 2: AWS Configure (Permanent)" -ForegroundColor Cyan
        Write-Host '  aws configure' -ForegroundColor White
        Write-Host "  Then enter your credentials when prompted" -ForegroundColor Gray
        Write-Host ""
        exit 1
    }
} catch {
    Write-Host "✗ Error checking AWS credentials" -ForegroundColor Red
    exit 1
}

# Configuration
$dbInstanceIdentifier = "insuranceloom-db"
$dbName = "insuranceloom"
$dbUsername = "postgres"
$dbPassword = ""  # Will prompt for password
$region = "af-south-1"
$vpcSecurityGroupId = ""  # Will be created or use existing

Write-Host ""
Write-Host "RDS Configuration:" -ForegroundColor Cyan
Write-Host "  Instance Identifier: $dbInstanceIdentifier"
Write-Host "  Database Name: $dbName"
Write-Host "  Master Username: $dbUsername"
Write-Host "  Region: $region"
Write-Host "  Instance Class: db.t3.micro (Free Tier)"
Write-Host "  Storage: 20 GB (Free Tier)"
Write-Host ""

# Prompt for database password
Write-Host "Enter a strong password for the database master user:" -ForegroundColor Yellow
Write-Host "(Password must be at least 8 characters with uppercase, lowercase, numbers, and special characters)" -ForegroundColor Gray
$securePassword = Read-Host "Database Password" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
$dbPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

if ($dbPassword.Length -lt 8) {
    Write-Host "✗ Password must be at least 8 characters" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Creating RDS PostgreSQL instance..." -ForegroundColor Yellow
Write-Host "This will take 5-10 minutes..." -ForegroundColor Gray
Write-Host ""

# Get default VPC ID
Write-Host "Getting default VPC..." -ForegroundColor Yellow
try {
    $vpcInfo = aws ec2 describe-vpcs --filters "Name=isDefault,Values=true" --region $region --query 'Vpcs[0].VpcId' --output text 2>&1
    $vpcId = $vpcInfo.Trim()
    Write-Host "✓ Using VPC: $vpcId" -ForegroundColor Green
} catch {
    Write-Host "✗ Error getting VPC: $_" -ForegroundColor Red
    Write-Host "Please check your AWS credentials and region" -ForegroundColor Yellow
    exit 1
}

# Get default subnet group or create one
Write-Host ""
Write-Host "Checking subnet group..." -ForegroundColor Yellow
$subnetGroupName = "default"
$subnetGroupExists = aws rds describe-db-subnet-groups --db-subnet-group-name $subnetGroupName --region $region 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Default subnet group not found, will use auto-created one" -ForegroundColor Gray
}

# Create security group for RDS
Write-Host ""
Write-Host "Creating security group for RDS..." -ForegroundColor Yellow
$securityGroupName = "insuranceloom-rds-sg"
$securityGroupDescription = "Security group for Insurance Loom RDS PostgreSQL"

try {
    $sgInfo = aws ec2 create-security-group `
        --group-name $securityGroupName `
        --description $securityGroupDescription `
        --vpc-id $vpcId `
        --region $region `
        --query 'GroupId' `
        --output text 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $vpcSecurityGroupId = $sgInfo.Trim()
        Write-Host "✓ Security group created: $vpcSecurityGroupId" -ForegroundColor Green
        
        # Get current public IP
        Write-Host "Getting your current public IP..." -ForegroundColor Yellow
        try {
            $publicIP = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content.Trim()
            $cidr = "$publicIP/32"
            Write-Host "✓ Your IP: $publicIP" -ForegroundColor Green
            
            # Add inbound rule for PostgreSQL
            Write-Host "Adding inbound rule for PostgreSQL (port 5432)..." -ForegroundColor Yellow
            aws ec2 authorize-security-group-ingress `
                --group-id $vpcSecurityGroupId `
                --protocol tcp `
                --port 5432 `
                --cidr $cidr `
                --region $region 2>&1 | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Inbound rule added for your IP ($cidr)" -ForegroundColor Green
            } else {
                Write-Host "⚠ Could not add inbound rule automatically. Please add manually in AWS Console." -ForegroundColor Yellow
            }
        } catch {
            Write-Host "⚠ Could not get your IP automatically. Please add inbound rule manually in AWS Console." -ForegroundColor Yellow
            Write-Host "  Security Group ID: $vpcSecurityGroupId" -ForegroundColor Gray
            Write-Host "  Port: 5432, Protocol: TCP" -ForegroundColor Gray
        }
    } else {
        # Security group might already exist
        $existingSG = aws ec2 describe-security-groups --filters "Name=group-name,Values=$securityGroupName" "Name=vpc-id,Values=$vpcId" --region $region --query 'SecurityGroups[0].GroupId' --output text 2>&1
        if ($LASTEXITCODE -eq 0 -and $existingSG -notmatch "None") {
            $vpcSecurityGroupId = $existingSG.Trim()
            Write-Host "✓ Using existing security group: $vpcSecurityGroupId" -ForegroundColor Green
        } else {
            Write-Host "✗ Error creating security group" -ForegroundColor Red
            exit 1
        }
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    exit 1
}

# Create RDS instance
Write-Host ""
Write-Host "Creating RDS PostgreSQL instance..." -ForegroundColor Yellow
Write-Host "Please wait, this takes 5-10 minutes..." -ForegroundColor Gray

$createDBCommand = @"
aws rds create-db-instance `
    --db-instance-identifier $dbInstanceIdentifier `
    --db-instance-class db.t3.micro `
    --engine postgres `
    --engine-version 15.5 `
    --master-username $dbUsername `
    --master-user-password $dbPassword `
    --allocated-storage 20 `
    --storage-type gp3 `
    --db-name $dbName `
    --backup-retention-period 7 `
    --publicly-accessible `
    --storage-encrypted `
    --no-multi-az `
    --no-deletion-protection `
    --region $region `
    --vpc-security-group-ids $vpcSecurityGroupId
"@

try {
    $result = Invoke-Expression $createDBCommand 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ RDS instance creation initiated!" -ForegroundColor Green
        Write-Host ""
        Write-Host "The database is being created. This will take 5-10 minutes." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To check the status, run:" -ForegroundColor Cyan
        Write-Host "  aws rds describe-db-instances --db-instance-identifier $dbInstanceIdentifier --region $region --query 'DBInstances[0].DBInstanceStatus' --output text" -ForegroundColor White
        Write-Host ""
        Write-Host "To get the endpoint when ready, run:" -ForegroundColor Cyan
        Write-Host "  aws rds describe-db-instances --db-instance-identifier $dbInstanceIdentifier --region $region --query 'DBInstances[0].Endpoint.Address' --output text" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host "✗ Error creating RDS instance:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        
        # Check if instance already exists
        if ($result -match "already exists") {
            Write-Host ""
            Write-Host "Instance already exists. Checking status..." -ForegroundColor Yellow
            $status = aws rds describe-db-instances --db-instance-identifier $dbInstanceIdentifier --region $region --query 'DBInstances[0].DBInstanceStatus' --output text 2>&1
            Write-Host "Status: $status" -ForegroundColor Cyan
        }
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Setup initiated successfully!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Wait for RDS instance to be 'available' (check status above)" -ForegroundColor White
Write-Host "2. Get the endpoint using the command above" -ForegroundColor White
Write-Host "3. Update appsettings.json with the endpoint and password" -ForegroundColor White
Write-Host "4. Run the migration script (see Data/Migrations/README.md)" -ForegroundColor White
Write-Host ""

