# Setup Local PostgreSQL for Development
# This script helps set up PostgreSQL for local development

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Local Development Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL is installed
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "❌ PostgreSQL is not installed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install PostgreSQL first:" -ForegroundColor Yellow
    Write-Host "1. Download from: https://www.postgresql.org/download/windows/" -ForegroundColor White
    Write-Host "2. Or use: winget install PostgreSQL.PostgreSQL" -ForegroundColor White
    Write-Host ""
    Write-Host "After installation, run this script again." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ PostgreSQL is installed" -ForegroundColor Green
Write-Host ""

# Prompt for PostgreSQL password
Write-Host "Enter your PostgreSQL 'postgres' user password:" -ForegroundColor Yellow
Write-Host "(This is the password you set during PostgreSQL installation)" -ForegroundColor Gray
$securePassword = Read-Host -AsSecureString
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
)

if ([string]::IsNullOrWhiteSpace($password)) {
    Write-Host "❌ Password cannot be empty" -ForegroundColor Red
    exit 1
}

# Set password environment variable
$env:PGPASSWORD = $password

Write-Host ""
Write-Host "Testing connection..." -ForegroundColor Yellow

# Test connection
$testConnection = & psql -h localhost -p 5432 -U postgres -d postgres -c "SELECT 1;" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Connection failed!" -ForegroundColor Red
    Write-Host "   Error: $testConnection" -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "1. Wrong password - try again" -ForegroundColor White
    Write-Host "2. PostgreSQL service not running" -ForegroundColor White
    Write-Host "3. PostgreSQL not installed correctly" -ForegroundColor White
    Write-Host ""
    Write-Host "Run: .\test-postgres-connection.ps1 for diagnostics" -ForegroundColor Yellow
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "✅ Connection successful!" -ForegroundColor Green
Write-Host ""

# Create database if it doesn't exist
Write-Host "Checking for database 'insuranceloom_dev'..." -ForegroundColor Yellow
$dbExists = & psql -h localhost -p 5432 -U postgres -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname='insuranceloom_dev';" 2>&1

if ($dbExists -match "1") {
    Write-Host "✅ Database 'insuranceloom_dev' already exists" -ForegroundColor Green
} else {
    Write-Host "Creating database 'insuranceloom_dev'..." -ForegroundColor Yellow
    $createDb = & psql -h localhost -p 5432 -U postgres -d postgres -c "CREATE DATABASE insuranceloom_dev;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database created successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to create database: $createDb" -ForegroundColor Red
        Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
        exit 1
    }
}

Write-Host ""

# Update appsettings.Development.json with the correct password
Write-Host "Updating appsettings.Development.json..." -ForegroundColor Yellow
$appSettingsPath = "appsettings.Development.json"

if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    
    # Update connection string
    $connectionString = "Host=localhost;Port=5432;Database=insuranceloom_dev;Username=postgres;Password=$password;Pooling=true;"
    $appSettings.ConnectionStrings.DefaultConnection = $connectionString
    
    # Save back to file
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
    Write-Host "✅ appsettings.Development.json updated" -ForegroundColor Green
    Write-Host "   Connection string configured with your password" -ForegroundColor Gray
} else {
    Write-Host "⚠️  appsettings.Development.json not found" -ForegroundColor Yellow
    Write-Host "   You may need to update it manually" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Run migrations: dotnet ef database update" -ForegroundColor White
Write-Host "   OR run the SQL script: psql -h localhost -U postgres -d insuranceloom_dev -f Data\Migrations\001_InitialSchema.sql" -ForegroundColor White
Write-Host "2. Test the API: dotnet run" -ForegroundColor White
Write-Host ""

# Clean up
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue

