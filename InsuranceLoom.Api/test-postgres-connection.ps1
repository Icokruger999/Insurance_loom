# PostgreSQL Connection Diagnostic Script
# This script helps diagnose PostgreSQL installation and connection issues

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Connection Diagnostic Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if PostgreSQL is installed
Write-Host "Step 1: Checking PostgreSQL Installation..." -ForegroundColor Yellow
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if ($psqlPath) {
    Write-Host "✅ PostgreSQL is installed" -ForegroundColor Green
    Write-Host "   Location: $($psqlPath.Source)" -ForegroundColor Gray
    $version = & psql --version 2>&1
    Write-Host "   Version: $version" -ForegroundColor Gray
} else {
    Write-Host "❌ PostgreSQL is NOT installed or not in PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "To install PostgreSQL:" -ForegroundColor Yellow
    Write-Host "1. Download from: https://www.postgresql.org/download/windows/" -ForegroundColor White
    Write-Host "2. Or use winget: winget install PostgreSQL.PostgreSQL" -ForegroundColor White
    Write-Host "3. Or use chocolatey: choco install postgresql15" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host ""

# Step 2: Check if PostgreSQL service is running
Write-Host "Step 2: Checking PostgreSQL Service Status..." -ForegroundColor Yellow
$pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
if ($pgService) {
    $runningService = $pgService | Where-Object { $_.Status -eq 'Running' }
    if ($runningService) {
        Write-Host "✅ PostgreSQL service is running" -ForegroundColor Green
        Write-Host "   Service: $($runningService.Name)" -ForegroundColor Gray
    } else {
        Write-Host "❌ PostgreSQL service is NOT running" -ForegroundColor Red
        Write-Host ""
        Write-Host "Attempting to start PostgreSQL service..." -ForegroundColor Yellow
        try {
            $pgService | Start-Service -ErrorAction Stop
            Start-Sleep -Seconds 3
            Write-Host "✅ PostgreSQL service started successfully" -ForegroundColor Green
        } catch {
            Write-Host "❌ Failed to start service: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "Try starting manually:" -ForegroundColor Yellow
            Write-Host "1. Open Services (services.msc)" -ForegroundColor White
            Write-Host "2. Find 'postgresql-x64-XX' service" -ForegroundColor White
            Write-Host "3. Right-click → Start" -ForegroundColor White
        }
    }
} else {
    Write-Host "⚠️  PostgreSQL service not found" -ForegroundColor Yellow
    Write-Host "   This might be normal if using a portable installation" -ForegroundColor Gray
}

Write-Host ""

# Step 3: Check if port 5432 is listening
Write-Host "Step 3: Checking Port 5432..." -ForegroundColor Yellow
$portCheck = Get-NetTCPConnection -LocalPort 5432 -ErrorAction SilentlyContinue
if ($portCheck) {
    Write-Host "✅ Port 5432 is listening" -ForegroundColor Green
    Write-Host "   State: $($portCheck.State)" -ForegroundColor Gray
} else {
    Write-Host "❌ Port 5432 is NOT listening" -ForegroundColor Red
    Write-Host "   PostgreSQL might not be running or using a different port" -ForegroundColor Gray
}

Write-Host ""

# Step 4: Test connection to localhost
Write-Host "Step 4: Testing Connection to Local PostgreSQL..." -ForegroundColor Yellow
Write-Host "   Attempting to connect to: localhost:5432" -ForegroundColor Gray
Write-Host "   Database: insuranceloom_dev" -ForegroundColor Gray
Write-Host "   Username: postgres" -ForegroundColor Gray
Write-Host ""

# Try to connect with default password
$env:PGPASSWORD = "postgres"
$connectionTest = & psql -h localhost -p 5432 -U postgres -d postgres -c "SELECT version();" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Connection successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Testing database creation..." -ForegroundColor Yellow
    
    # Check if database exists
    $dbExists = & psql -h localhost -p 5432 -U postgres -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname='insuranceloom_dev';" 2>&1
    if ($dbExists -match "1") {
        Write-Host "✅ Database 'insuranceloom_dev' already exists" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Database 'insuranceloom_dev' does not exist" -ForegroundColor Yellow
        Write-Host "   Creating database..." -ForegroundColor Yellow
        $createDb = & psql -h localhost -p 5432 -U postgres -d postgres -c "CREATE DATABASE insuranceloom_dev;" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Database created successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to create database: $createDb" -ForegroundColor Red
        }
    }
} else {
    Write-Host "❌ Connection failed!" -ForegroundColor Red
    Write-Host "   Error: $connectionTest" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues and solutions:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Wrong Password:" -ForegroundColor White
    Write-Host "   - Default password might be different" -ForegroundColor Gray
    Write-Host "   - Check what you set during installation" -ForegroundColor Gray
    Write-Host "   - Try: psql -h localhost -U postgres" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. PostgreSQL Not Running:" -ForegroundColor White
    Write-Host "   - Start PostgreSQL service (see Step 2 above)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. Wrong Port:" -ForegroundColor White
    Write-Host "   - Check if PostgreSQL is on a different port" -ForegroundColor Gray
    Write-Host "   - Check postgresql.conf file" -ForegroundColor Gray
    Write-Host ""
    Write-Host "4. Authentication Failed:" -ForegroundColor White
    Write-Host "   - Check pg_hba.conf file" -ForegroundColor Gray
    Write-Host "   - Location: C:\Program Files\PostgreSQL\15\data\pg_hba.conf" -ForegroundColor Gray
    Write-Host ""
    
    # Try to get more info
    Write-Host "Trying to get more information..." -ForegroundColor Yellow
    Write-Host "   Full error output:" -ForegroundColor Gray
    $connectionTest | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Diagnostic Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Clean up
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue

