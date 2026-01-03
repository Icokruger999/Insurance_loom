# AWS S3 Bucket Setup Script for Insurance Loom
# This script creates the S3 bucket for document storage

$bucketName = "insurance-loom-documents"
$region = "af-south-1"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Insurance Loom - AWS S3 Bucket Setup" -ForegroundColor Cyan
Write-Host "Bucket: $bucketName" -ForegroundColor Cyan
Write-Host "Region: $region" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if AWS CLI is installed
$awsCheck = aws --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ AWS CLI not found. Please install AWS CLI first:" -ForegroundColor Red
    Write-Host "  https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✓ AWS CLI found: $awsCheck" -ForegroundColor Green
}

# Check if credentials are configured
Write-Host ""
Write-Host "Checking AWS credentials..." -ForegroundColor Yellow
$null = aws sts get-caller-identity 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ AWS credentials are configured" -ForegroundColor Green
} else {
    Write-Host "✗ AWS credentials not configured" -ForegroundColor Red
    Write-Host "  Please configure AWS credentials using:" -ForegroundColor Yellow
    Write-Host "  aws configure" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Checking if bucket already exists..." -ForegroundColor Yellow

# Check if bucket exists
$bucketExists = aws s3api head-bucket --bucket $bucketName --region $region 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Bucket '$bucketName' already exists" -ForegroundColor Green
    Write-Host ""
    Write-Host "Bucket Details:" -ForegroundColor Cyan
    $bucketLocation = aws s3api get-bucket-location --bucket $bucketName --region $region --query 'LocationConstraint' --output text 2>&1
    Write-Host "  Bucket Name: $bucketName" -ForegroundColor White
    Write-Host "  Region: $bucketLocation" -ForegroundColor White
    Write-Host ""
    Write-Host "You can verify bucket settings in AWS Console:" -ForegroundColor Yellow
    Write-Host "  https://s3.console.aws.amazon.com/s3/buckets/$bucketName" -ForegroundColor White
} else {
    Write-Host "Bucket does not exist. Creating bucket..." -ForegroundColor Yellow
    Write-Host ""
    
    # Create bucket
    Write-Host "Creating S3 bucket: $bucketName" -ForegroundColor Cyan
    
    # For af-south-1, we need to specify the location constraint
    $createResult = aws s3api create-bucket `
        --bucket $bucketName `
        --region $region `
        --create-bucket-configuration LocationConstraint=$region 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Bucket created successfully!" -ForegroundColor Green
        Write-Host ""
        
        # Enable versioning
        Write-Host "Enabling versioning..." -ForegroundColor Yellow
        $versionResult = aws s3api put-bucket-versioning `
            --bucket $bucketName `
            --versioning-configuration Status=Enabled `
            --region $region 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Versioning enabled" -ForegroundColor Green
        } else {
            Write-Host "⚠ Could not enable versioning: $versionResult" -ForegroundColor Yellow
        }
        
        # Enable encryption
        Write-Host "Enabling server-side encryption..." -ForegroundColor Yellow
        $encryptionConfig = '{"Rules":[{"ApplyServerSideEncryptionByDefault":{"SSEAlgorithm":"AES256"},"BucketKeyEnabled":true}]}'
        
        $encryptionResult = aws s3api put-bucket-encryption `
            --bucket $bucketName `
            --server-side-encryption-configuration $encryptionConfig `
            --region $region 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Encryption enabled (AES256)" -ForegroundColor Green
        } else {
            Write-Host "⚠ Could not enable encryption: $encryptionResult" -ForegroundColor Yellow
        }
        
        # Block public access (default, but let's be explicit)
        Write-Host "Blocking public access..." -ForegroundColor Yellow
        $blockPublicConfig = '{"BlockPublicAcls":true,"IgnorePublicAcls":true,"BlockPublicPolicy":true,"RestrictPublicBuckets":true}'
        
        $blockResult = aws s3api put-public-access-block `
            --bucket $bucketName `
            --public-access-block-configuration $blockPublicConfig `
            --region $region 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Public access blocked" -ForegroundColor Green
        } else {
            Write-Host "⚠ Could not block public access: $blockResult" -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Host "======================================" -ForegroundColor Green
        Write-Host "✓ S3 Bucket Setup Complete!" -ForegroundColor Green
        Write-Host "======================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Bucket Details:" -ForegroundColor Cyan
        Write-Host "  Bucket Name: $bucketName" -ForegroundColor White
        Write-Host "  Region: $region" -ForegroundColor White
        Write-Host "  Versioning: Enabled" -ForegroundColor White
        Write-Host "  Encryption: AES256" -ForegroundColor White
        Write-Host "  Public Access: Blocked" -ForegroundColor White
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Yellow
        Write-Host "1. Create an IAM user with S3 permissions (see instructions below)" -ForegroundColor White
        Write-Host "2. Get Access Key and Secret Key from IAM user" -ForegroundColor White
        Write-Host "3. Update appsettings.json with:" -ForegroundColor White
        Write-Host "   - S3Bucket: $bucketName" -ForegroundColor Gray
        Write-Host "   - AccessKey: [your-access-key]" -ForegroundColor Gray
        Write-Host "   - SecretKey: [your-secret-key]" -ForegroundColor Gray
        Write-Host ""
        Write-Host "View bucket in AWS Console:" -ForegroundColor Yellow
        Write-Host "  https://s3.console.aws.amazon.com/s3/buckets/$bucketName" -ForegroundColor White
    } else {
        Write-Host "✗ Error creating bucket: $createResult" -ForegroundColor Red
        Write-Host ""
        Write-Host "Common issues:" -ForegroundColor Yellow
        Write-Host "  - Bucket name already exists (must be globally unique)" -ForegroundColor White
        Write-Host "  - Insufficient permissions (need s3:CreateBucket)" -ForegroundColor White
        Write-Host "  - Invalid bucket name (must be lowercase, no spaces)" -ForegroundColor White
        exit 1
    }
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "IAM User Setup Instructions" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To allow your API to access S3, create an IAM user:" -ForegroundColor White
Write-Host ""
Write-Host "1. Go to IAM Console: https://console.aws.amazon.com/iam/" -ForegroundColor Yellow
Write-Host "2. Click 'Users' → 'Create user'" -ForegroundColor White
Write-Host "3. Username: insurance-loom-api-s3" -ForegroundColor White
Write-Host "4. Select 'Attach policies directly'" -ForegroundColor White
Write-Host "5. Attach policy: AmazonS3FullAccess (or create custom policy below)" -ForegroundColor White
Write-Host "6. Create user and save Access Key ID and Secret Access Key" -ForegroundColor White
Write-Host ""
Write-Host "Custom IAM Policy (more secure, only for this bucket):" -ForegroundColor Yellow
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
Write-Host $policyJson -ForegroundColor Gray
Write-Host ""

