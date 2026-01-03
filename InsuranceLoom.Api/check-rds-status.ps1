# Check RDS Status and Get Endpoint
# This script checks the RDS instance status and displays connection details when ready

$env:AWS_ACCESS_KEY_ID="YOUR_AWS_ACCESS_KEY_ID"
$env:AWS_SECRET_ACCESS_KEY="YOUR_AWS_SECRET_ACCESS_KEY"
$env:AWS_DEFAULT_REGION="af-south-1"

$dbInstanceIdentifier = "insuranceloom-db"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Checking RDS Instance Status" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

try {
    $status = aws rds describe-db-instances `
        --db-instance-identifier $dbInstanceIdentifier `
        --region af-south-1 `
        --query 'DBInstances[0].DBInstanceStatus' `
        --output text 2>&1
    
    if ($LASTEXITCODE -eq 0 -and $status -ne "None") {
        Write-Host "Status: $status" -ForegroundColor $(if ($status -eq "available") { "Green" } elseif ($status -eq "creating") { "Yellow" } else { "Red" })
        Write-Host ""
        
        if ($status -eq "available") {
            # Get endpoint and port
            $endpoint = aws rds describe-db-instances `
                --db-instance-identifier $dbInstanceIdentifier `
                --region af-south-1 `
                --query 'DBInstances[0].Endpoint.Address' `
                --output text 2>&1
            
            $port = aws rds describe-db-instances `
                --db-instance-identifier $dbInstanceIdentifier `
                --region af-south-1 `
                --query 'DBInstances[0].Endpoint.Port' `
                --output text 2>&1
            
            Write-Host "✅ RDS Instance is READY!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Connection Details:" -ForegroundColor Cyan
            Write-Host "  Endpoint: $endpoint" -ForegroundColor White
            Write-Host "  Port: $port" -ForegroundColor White
            Write-Host "  Database: insuranceloom" -ForegroundColor White
            Write-Host "  Username: postgres" -ForegroundColor White
            Write-Host "  Password: [YOUR_DB_PASSWORD]" -ForegroundColor White
            Write-Host ""
            Write-Host "Connection String:" -ForegroundColor Cyan
            Write-Host "Host=$endpoint;Port=$port;Database=insuranceloom;Username=postgres;Password=[YOUR_DB_PASSWORD];Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Next Steps:" -ForegroundColor Cyan
            Write-Host "1. Update appsettings.json with the connection string above" -ForegroundColor White
            Write-Host "2. Run migration script: Data/Migrations/001_InitialSchema.sql" -ForegroundColor White
            Write-Host "3. Test connection: dotnet run" -ForegroundColor White
        } elseif ($status -eq "creating") {
            Write-Host "⏳ RDS instance is still being created..." -ForegroundColor Yellow
            Write-Host "This usually takes 5-10 minutes. Please wait and run this script again." -ForegroundColor Gray
            Write-Host ""
            Write-Host "To check status manually:" -ForegroundColor Cyan
            Write-Host "  aws rds describe-db-instances --db-instance-identifier $dbInstanceIdentifier --region af-south-1 --query 'DBInstances[0].DBInstanceStatus' --output text" -ForegroundColor White
        } else {
            Write-Host "⚠️  Unexpected status: $status" -ForegroundColor Red
            Write-Host "Check AWS Console for more details." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Could not retrieve RDS instance status" -ForegroundColor Red
        Write-Host "Make sure the instance exists: $dbInstanceIdentifier" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
}

