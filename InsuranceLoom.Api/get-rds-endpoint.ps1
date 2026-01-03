# Quick script to get RDS endpoint and connection details

$dbInstanceIdentifier = "insuranceloom-db"
$region = "af-south-1"

Write-Host "Getting RDS instance details..." -ForegroundColor Cyan
Write-Host ""

try {
    # Get instance status
    $status = aws rds describe-db-instances `
        --db-instance-identifier $dbInstanceIdentifier `
        --region $region `
        --query 'DBInstances[0].DBInstanceStatus' `
        --output text 2>&1
    
    if ($LASTEXITCODE -eq 0 -and $status -ne "None") {
        Write-Host "Status: $status" -ForegroundColor $(if ($status -eq "available") { "Green" } else { "Yellow" })
        Write-Host ""
        
        if ($status -eq "available") {
            # Get endpoint
            $endpoint = aws rds describe-db-instances `
                --db-instance-identifier $dbInstanceIdentifier `
                --region $region `
                --query 'DBInstances[0].Endpoint.Address' `
                --output text 2>&1
            
            $port = aws rds describe-db-instances `
                --db-instance-identifier $dbInstanceIdentifier `
                --region $region `
                --query 'DBInstances[0].Endpoint.Port' `
                --output text 2>&1
            
            Write-Host "Connection Details:" -ForegroundColor Green
            Write-Host "  Endpoint: $endpoint" -ForegroundColor White
            Write-Host "  Port: $port" -ForegroundColor White
            Write-Host "  Database: insuranceloom" -ForegroundColor White
            Write-Host "  Username: postgres" -ForegroundColor White
            Write-Host ""
            Write-Host "Connection String (replace YOUR_PASSWORD_HERE):" -ForegroundColor Cyan
            Write-Host "Host=$endpoint;Port=$port;Database=insuranceloom;Username=postgres;Password=YOUR_PASSWORD_HERE;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;" -ForegroundColor Yellow
        } else {
            Write-Host "Instance is not available yet. Current status: $status" -ForegroundColor Yellow
            Write-Host "Please wait and run this script again in a few minutes." -ForegroundColor Gray
        }
    } else {
        Write-Host "✗ RDS instance not found or error occurred" -ForegroundColor Red
        Write-Host "Make sure the instance identifier is correct: $dbInstanceIdentifier" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure:" -ForegroundColor Yellow
    Write-Host "1. AWS CLI is installed and configured" -ForegroundColor White
    Write-Host "2. AWS credentials are set correctly" -ForegroundColor White
    Write-Host "3. Region is correct: $region" -ForegroundColor White
}

