# Simple RDS Setup Script
# Run this after setting AWS credentials

$env:AWS_ACCESS_KEY_ID="YOUR_AWS_ACCESS_KEY_ID"
$env:AWS_SECRET_ACCESS_KEY="YOUR_AWS_SECRET_ACCESS_KEY"
$env:AWS_DEFAULT_REGION="af-south-1"

Write-Host "Testing AWS credentials..." -ForegroundColor Yellow
$test = aws sts get-caller-identity 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Invalid AWS credentials" -ForegroundColor Red
    Write-Host $test -ForegroundColor Red
    Write-Host ""
    Write-Host "Please verify:" -ForegroundColor Yellow
    Write-Host "1. Access Key ID is correct" -ForegroundColor White
    Write-Host "2. Secret Access Key is correct (check for typos)" -ForegroundColor White
    Write-Host "3. Credentials are active in AWS IAM" -ForegroundColor White
    exit 1
}

Write-Host "Credentials OK!" -ForegroundColor Green
Write-Host ""
Write-Host "Note: This script will prompt you for a database password." -ForegroundColor Cyan
Write-Host "Please run the full setup script: setup-aws-rds.ps1" -ForegroundColor Yellow

