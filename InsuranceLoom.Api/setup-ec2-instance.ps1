# Setup EC2 Instance for Insurance Loom API
# This script helps create and configure an EC2 instance

param(
    [string]$InstanceType = "t2.micro",
    [string]$KeyName = "insuranceloom-api-key",
    [string]$Region = "af-south-1"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "EC2 Instance Setup for Insurance Loom API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check AWS CLI
if (-not (Get-Command aws -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: AWS CLI is not installed!" -ForegroundColor Red
    Write-Host "Install it from: https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
}

# Check AWS credentials
Write-Host "Checking AWS credentials..." -ForegroundColor Yellow
$identity = aws sts get-caller-identity 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: AWS credentials not configured!" -ForegroundColor Red
    Write-Host "Set credentials using:" -ForegroundColor Yellow
    Write-Host "  `$env:AWS_ACCESS_KEY_ID='your-key'" -ForegroundColor Gray
    Write-Host "  `$env:AWS_SECRET_ACCESS_KEY='your-secret'" -ForegroundColor Gray
    Write-Host "  `$env:AWS_DEFAULT_REGION='$Region'" -ForegroundColor Gray
    exit 1
}

Write-Host "✅ AWS credentials configured" -ForegroundColor Green
Write-Host ""

# Get current region
$currentRegion = aws configure get region
if ([string]::IsNullOrEmpty($currentRegion)) {
    Write-Host "Setting region to $Region..." -ForegroundColor Yellow
    aws configure set region $Region
} else {
    $Region = $currentRegion
    Write-Host "Using region: $Region" -ForegroundColor Green
}

Write-Host ""
Write-Host "This script will help you:" -ForegroundColor Cyan
Write-Host "  1. Create a security group" -ForegroundColor White
Write-Host "  2. Create a key pair (if needed)" -ForegroundColor White
Write-Host "  3. Launch an EC2 instance" -ForegroundColor White
Write-Host "  4. Get connection information" -ForegroundColor White
Write-Host ""
Write-Host "Note: This creates a basic setup. You'll need to:" -ForegroundColor Yellow
Write-Host "  - SSH into the instance" -ForegroundColor Gray
Write-Host "  - Install .NET runtime" -ForegroundColor Gray
Write-Host "  - Install and configure Nginx" -ForegroundColor Gray
Write-Host "  - Deploy your API" -ForegroundColor Gray
Write-Host "  - Set up SSL certificate" -ForegroundColor Gray
Write-Host ""
$continue = Read-Host "Continue? (y/n)"
if ($continue -ne "y" -and $continue -ne "Y") {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

# Step 1: Create Security Group
Write-Host ""
Write-Host "Step 1: Creating Security Group..." -ForegroundColor Cyan
$sgName = "insuranceloom-api-sg"
$sgDescription = "Security group for Insurance Loom API"

# Check if security group already exists
$existingSG = aws ec2 describe-security-groups --group-names $sgName --region $Region 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Security group '$sgName' already exists" -ForegroundColor Yellow
    $sgId = (aws ec2 describe-security-groups --group-names $sgName --region $Region --query 'SecurityGroups[0].GroupId' --output text)
    Write-Host "Using existing security group: $sgId" -ForegroundColor Green
} else {
    # Create security group
    Write-Host "Creating security group..." -ForegroundColor Yellow
    $sgResult = aws ec2 create-security-group --group-name $sgName --description $sgDescription --region $Region 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR creating security group: $sgResult" -ForegroundColor Red
        exit 1
    }
    $sgId = ($sgResult | ConvertFrom-Json).GroupId
    Write-Host "✅ Security group created: $sgId" -ForegroundColor Green
    
    # Add rules
    Write-Host "Adding security group rules..." -ForegroundColor Yellow
    
    # SSH from anywhere (you should restrict this later)
    aws ec2 authorize-security-group-ingress --group-id $sgId --protocol tcp --port 22 --cidr 0.0.0.0/0 --region $Region | Out-Null
    Write-Host "  ✅ SSH (22) - Open (restrict this later!)" -ForegroundColor Green
    
    # HTTP
    aws ec2 authorize-security-group-ingress --group-id $sgId --protocol tcp --port 80 --cidr 0.0.0.0/0 --region $Region | Out-Null
    Write-Host "  ✅ HTTP (80)" -ForegroundColor Green
    
    # HTTPS
    aws ec2 authorize-security-group-ingress --group-id $sgId --protocol tcp --port 443 --cidr 0.0.0.0/0 --region $Region | Out-Null
    Write-Host "  ✅ HTTPS (443)" -ForegroundColor Green
    
    # API port (temporary, remove after nginx setup)
    aws ec2 authorize-security-group-ingress --group-id $sgId --protocol tcp --port 5000 --cidr 0.0.0.0/0 --region $Region | Out-Null
    Write-Host "  ✅ API (5000) - Temporary, remove after nginx setup" -ForegroundColor Yellow
}

# Step 2: Create Key Pair
Write-Host ""
Write-Host "Step 2: Setting up Key Pair..." -ForegroundColor Cyan

# Check if key pair exists
$existingKey = aws ec2 describe-key-pairs --key-names $KeyName --region $Region 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Key pair '$KeyName' already exists" -ForegroundColor Yellow
    Write-Host "⚠️  Make sure you have the .pem file for this key!" -ForegroundColor Yellow
} else {
    Write-Host "Creating key pair: $KeyName" -ForegroundColor Yellow
    $keyFile = "$KeyName.pem"
    aws ec2 create-key-pair --key-name $KeyName --region $Region --query 'KeyMaterial' --output text | Out-File -FilePath $keyFile -Encoding ASCII
    
    if (Test-Path $keyFile) {
        # Set permissions (Unix-style, but Windows will ignore)
        icacls $keyFile /inheritance:r /grant:r "$env:USERNAME:(R)" | Out-Null
        Write-Host "✅ Key pair created: $keyFile" -ForegroundColor Green
        Write-Host "⚠️  IMPORTANT: Save this file securely! You'll need it to SSH into the instance." -ForegroundColor Yellow
        Write-Host "   Location: $(Resolve-Path $keyFile)" -ForegroundColor Gray
    } else {
        Write-Host "ERROR: Failed to create key file" -ForegroundColor Red
        exit 1
    }
}

# Step 3: Get Latest AMI
Write-Host ""
Write-Host "Step 3: Getting Latest Amazon Linux 2023 AMI..." -ForegroundColor Cyan
$amiId = aws ec2 describe-images --owners amazon --filters "Name=name,Values=al2023-ami-*-x86_64" "Name=state,Values=available" --query 'Images | sort_by(@, &CreationDate) | [-1].ImageId' --output text --region $Region
if ([string]::IsNullOrEmpty($amiId)) {
    Write-Host "ERROR: Could not find Amazon Linux 2023 AMI" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Using AMI: $amiId" -ForegroundColor Green

# Step 4: Launch Instance
Write-Host ""
Write-Host "Step 4: Launching EC2 Instance..." -ForegroundColor Cyan
Write-Host "Instance Type: $InstanceType" -ForegroundColor White
Write-Host "Security Group: $sgId" -ForegroundColor White
Write-Host "Key Pair: $KeyName" -ForegroundColor White
Write-Host ""

$userData = @"
#!/bin/bash
# Update system
dnf update -y
# Install .NET 8 Runtime
dnf install -y dotnet-runtime-8.0
# Install Git
dnf install -y git
"@

$userDataBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($userData))

Write-Host "Launching instance (this may take a minute)..." -ForegroundColor Yellow
$instanceResult = aws ec2 run-instances `
    --image-id $amiId `
    --instance-type $InstanceType `
    --key-name $KeyName `
    --security-group-ids $sgId `
    --user-data $userDataBase64 `
    --tag-specifications "ResourceType=instance,Tags=[{Key=Name,Value=insuranceloom-api}]" `
    --region $Region `
    --output json

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to launch instance" -ForegroundColor Red
    exit 1
}

$instanceId = ($instanceResult | ConvertFrom-Json).Instances[0].InstanceId
Write-Host "✅ Instance launched: $instanceId" -ForegroundColor Green

# Wait for instance to be running
Write-Host ""
Write-Host "Waiting for instance to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10
aws ec2 wait instance-running --instance-ids $instanceId --region $Region
Write-Host "✅ Instance is running" -ForegroundColor Green

# Get Public IP
Write-Host ""
Write-Host "Getting instance information..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
$instanceInfo = aws ec2 describe-instances --instance-ids $instanceId --region $Region --query 'Reservations[0].Instances[0]' --output json | ConvertFrom-Json
$publicIP = $instanceInfo.PublicIpAddress
$publicDNS = $instanceInfo.PublicDnsName

# Output Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ EC2 Instance Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Instance Details:" -ForegroundColor Cyan
Write-Host "  Instance ID: $instanceId" -ForegroundColor White
Write-Host "  Public IP: $publicIP" -ForegroundColor White
Write-Host "  Public DNS: $publicDNS" -ForegroundColor White
Write-Host "  Region: $Region" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. SSH into the instance:" -ForegroundColor White
Write-Host "     ssh -i `"$KeyName.pem`" ec2-user@$publicIP" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Once connected, install .NET (if not auto-installed):" -ForegroundColor White
Write-Host "     sudo dnf install -y dotnet-runtime-8.0" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Follow the EC2_DEPLOYMENT_GUIDE.md for:" -ForegroundColor White
Write-Host "     - Installing Nginx" -ForegroundColor Gray
Write-Host "     - Setting up SSL" -ForegroundColor Gray
Write-Host "     - Deploying your API" -ForegroundColor Gray
Write-Host ""
Write-Host "⚠️  Security Note:" -ForegroundColor Yellow
Write-Host "  - Restrict SSH (port 22) to your IP only" -ForegroundColor Gray
Write-Host "  - Remove port 5000 after setting up Nginx" -ForegroundColor Gray
Write-Host "  - Keep your .pem key file secure!" -ForegroundColor Gray
Write-Host ""

