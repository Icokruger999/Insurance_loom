# AWS IAM User Setup Script for Insurance Loom S3 Access
# This script creates an IAM user with S3 permissions for the API

$userName = "insurance-loom-api-s3"
$bucketName = "insurance-loom-documents"
$policyName = "InsuranceLoomS3BucketPolicy"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Insurance Loom - IAM User Setup" -ForegroundColor Cyan
Write-Host "User: $userName" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if AWS CLI is installed
$awsCheck = aws --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: AWS CLI not found" -ForegroundColor Red
    Write-Host "  Please install AWS CLI first: https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "OK: AWS CLI found: $awsCheck" -ForegroundColor Green
}

# Check if credentials are configured
Write-Host ""
Write-Host "Checking AWS credentials..." -ForegroundColor Yellow
$null = aws sts get-caller-identity 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: AWS credentials not configured" -ForegroundColor Red
    Write-Host "  Please configure AWS credentials using: aws configure" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "OK: AWS credentials are configured" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 1: Checking if IAM user already exists..." -ForegroundColor Cyan
$userExists = aws iam get-user --user-name $userName 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "WARNING: IAM user '$userName' already exists" -ForegroundColor Yellow
    Write-Host "  Skipping user creation..." -ForegroundColor Gray
} else {
    Write-Host "Creating IAM user: $userName" -ForegroundColor Yellow
    $createUser = aws iam create-user --user-name $userName 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: IAM user created successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to create user: $createUser" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Step 2: Creating custom S3 policy..." -ForegroundColor Cyan

# Create custom policy JSON
$policyJson = @"
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "s3:PutObject",
                "s3:GetObject",
                "s3:DeleteObject",
                "s3:ListBucket"
            ],
            "Resource": [
                "arn:aws:s3:::$bucketName",
                "arn:aws:s3:::$bucketName/*"
            ]
        }
    ]
}
"@

# Save policy to temp file
$policyFile = "$env:TEMP\insurance-loom-s3-policy.json"
$policyJson | Out-File -FilePath $policyFile -Encoding UTF8

# Get account ID
$accountId = aws sts get-caller-identity --query Account --output text
$policyArn = "arn:aws:iam::${accountId}:policy/$policyName"

# Check if policy already exists
$policyExists = aws iam get-policy --policy-arn $policyArn 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "WARNING: Policy '$policyName' already exists" -ForegroundColor Yellow
    Write-Host "  Skipping policy creation..." -ForegroundColor Gray
} else {
    Write-Host "Creating policy: $policyName" -ForegroundColor Yellow
    $createPolicy = aws iam create-policy --policy-name $policyName --policy-document "file://$policyFile" --description "Allows S3 access to insurance-loom-documents bucket only" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Policy created successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to create policy: $createPolicy" -ForegroundColor Red
        Remove-Item $policyFile -ErrorAction SilentlyContinue
        exit 1
    }
}

Write-Host ""
Write-Host "Step 3: Attaching policy to user..." -ForegroundColor Cyan

# Check if policy is already attached
$attachedPolicies = aws iam list-attached-user-policies --user-name $userName --query "AttachedPolicies[?PolicyArn=='$policyArn'].PolicyArn" --output text 2>&1
if ($attachedPolicies -eq $policyArn) {
    Write-Host "OK: Policy already attached to user" -ForegroundColor Yellow
} else {
    Write-Host "Attaching policy to user..." -ForegroundColor Yellow
    $attachPolicy = aws iam attach-user-policy --user-name $userName --policy-arn $policyArn 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Policy attached successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to attach policy: $attachPolicy" -ForegroundColor Red
        Remove-Item $policyFile -ErrorAction SilentlyContinue
        exit 1
    }
}

# Clean up temp file
Remove-Item $policyFile -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Step 4: Creating access keys..." -ForegroundColor Cyan

# Check if access keys already exist
$existingKeys = aws iam list-access-keys --user-name $userName --query "AccessKeyMetadata[?Status=='Active'].AccessKeyId" --output text 2>&1
if ($existingKeys -and $existingKeys -ne "") {
    Write-Host "WARNING: Access keys already exist for this user:" -ForegroundColor Yellow
    Write-Host "  Access Key IDs: $existingKeys" -ForegroundColor White
    Write-Host ""
    Write-Host "Do you want to create a new access key? (y/n)" -ForegroundColor Yellow
    $response = Read-Host
    if ($response -ne "y" -and $response -ne "Y") {
        Write-Host "Skipping access key creation. Using existing keys." -ForegroundColor Gray
        Write-Host ""
        Write-Host "To view existing access keys, go to:" -ForegroundColor Yellow
        Write-Host "  https://console.aws.amazon.com/iam/home#/users/$userName?section=security_credentials" -ForegroundColor White
        exit 0
    }
}

Write-Host "Creating new access key..." -ForegroundColor Yellow
$accessKeyOutput = aws iam create-access-key --user-name $userName 2>&1
if ($LASTEXITCODE -eq 0) {
    $accessKeyResult = $accessKeyOutput | ConvertFrom-Json
    $accessKeyId = $accessKeyResult.AccessKey.AccessKeyId
    $secretAccessKey = $accessKeyResult.AccessKey.SecretAccessKey
    
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Green
    Write-Host "SUCCESS: IAM User Setup Complete!" -ForegroundColor Green
    Write-Host "======================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANT: Save these credentials now!" -ForegroundColor Red
    Write-Host "   You will NOT be able to see the Secret Access Key again!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Access Key ID:" -ForegroundColor Cyan
    Write-Host "  $accessKeyId" -ForegroundColor White
    Write-Host ""
    Write-Host "Secret Access Key:" -ForegroundColor Cyan
    Write-Host "  $secretAccessKey" -ForegroundColor White
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Yellow
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "======================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Copy the Access Key ID and Secret Access Key above" -ForegroundColor White
    Write-Host "2. Update InsuranceLoom.Api/appsettings.json" -ForegroundColor White
    Write-Host ""
    Write-Host "View user in AWS Console:" -ForegroundColor Yellow
    Write-Host "  https://console.aws.amazon.com/iam/home#/users/$userName" -ForegroundColor White
} else {
    Write-Host "ERROR: Failed to create access key: $accessKeyOutput" -ForegroundColor Red
    exit 1
}
