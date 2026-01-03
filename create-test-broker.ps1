# Script to create a test broker account
# This script will insert a test broker into the database

$email = "testbroker@insuranceloom.com"
$password = "Test123!"
$agentNumber = "BROKER001"
$firstName = "Test"
$lastName = "Broker"
$companyName = "Test Insurance Agency"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Creating Test Broker Account" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Email: $email" -ForegroundColor White
Write-Host "Password: $password" -ForegroundColor White
Write-Host "Agent Number: $agentNumber" -ForegroundColor White
Write-Host ""

# Check if .NET is available
$dotnetCheck = dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK not found" -ForegroundColor Red
    Write-Host "  Please install .NET SDK to hash the password" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternative: You can manually hash the password using an online BCrypt tool" -ForegroundColor Yellow
    Write-Host "  Password: $password" -ForegroundColor White
    exit 1
}

Write-Host "OK: .NET SDK found: $dotnetCheck" -ForegroundColor Green
Write-Host ""

# Create a temporary C# script to hash the password
$hashScript = @"
using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "$password";
        string hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        Console.WriteLine(hash);
    }
}
"@

$tempScript = "$env:TEMP\hash-password.cs"
$hashScript | Out-File -FilePath $tempScript -Encoding UTF8

# Create a temporary project file
$projectFile = "$env:TEMP\HashPassword.csproj"
$projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  </ItemGroup>
</Project>
"@
$projectContent | Out-File -FilePath $projectFile -Encoding UTF8

Write-Host "Hashing password..." -ForegroundColor Yellow
Set-Location $env:TEMP

# Create project and hash password
$null = dotnet new console -n HashPassword -f 2>&1
if (Test-Path "HashPassword") {
    Remove-Item -Recurse -Force "HashPassword" -ErrorAction SilentlyContinue
}
$null = dotnet new console -n HashPassword -f 2>&1
Copy-Item $tempScript "HashPassword\Program.cs" -Force
Copy-Item $projectFile "HashPassword\HashPassword.csproj" -Force

Set-Location "HashPassword"
$hashOutput = dotnet run 2>&1
$passwordHash = $hashOutput | Select-Object -Last 1

Set-Location $PSScriptRoot
Remove-Item -Recurse -Force "$env:TEMP\HashPassword" -ErrorAction SilentlyContinue
Remove-Item $tempScript -ErrorAction SilentlyContinue
Remove-Item $projectFile -ErrorAction SilentlyContinue

if (-not $passwordHash -or $passwordHash.Length -lt 20) {
    Write-Host "ERROR: Failed to hash password" -ForegroundColor Red
    Write-Host "  Output: $hashOutput" -ForegroundColor Yellow
    exit 1
}

Write-Host "OK: Password hashed successfully" -ForegroundColor Green
Write-Host ""

# Generate UUIDs
$userId = [guid]::NewGuid()
$brokerId = [guid]::NewGuid()

# Create SQL script
$sqlScript = @"
-- Test Broker Account
-- Email: $email
-- Password: $password
-- Agent Number: $agentNumber

-- Insert User
INSERT INTO users (id, email, password_hash, user_type, is_active, created_at, updated_at)
VALUES (
    '$userId'::uuid,
    '$email',
    '$passwordHash',
    'Broker',
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (email) DO UPDATE SET
    password_hash = EXCLUDED.password_hash,
    updated_at = CURRENT_TIMESTAMP;

-- Insert Broker
INSERT INTO brokers (id, user_id, agent_number, first_name, last_name, company_name, phone, is_active, created_at, updated_at)
VALUES (
    '$brokerId'::uuid,
    '$userId'::uuid,
    '$agentNumber',
    '$firstName',
    '$lastName',
    '$companyName',
    '+27123456789',
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (agent_number) DO UPDATE SET
    first_name = EXCLUDED.first_name,
    last_name = EXCLUDED.last_name,
    company_name = EXCLUDED.company_name,
    updated_at = CURRENT_TIMESTAMP;

-- Verify the broker was created
SELECT 
    b.agent_number,
    b.first_name,
    b.last_name,
    u.email,
    u.user_type,
    u.is_active
FROM brokers b
JOIN users u ON b.user_id = u.id
WHERE b.agent_number = '$agentNumber';
"@

$sqlFile = "create-test-broker.sql"
$sqlScript | Out-File -FilePath $sqlFile -Encoding UTF8

Write-Host "======================================" -ForegroundColor Green
Write-Host "Test Broker SQL Script Created!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""
Write-Host "SQL file created: $sqlFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "Login Credentials:" -ForegroundColor Yellow
Write-Host "  Agent Number: $agentNumber" -ForegroundColor White
Write-Host "  Password: $password" -ForegroundColor White
Write-Host "  Email: $email" -ForegroundColor White
Write-Host ""
Write-Host "To insert this broker into your database:" -ForegroundColor Yellow
Write-Host "  1. Get your RDS endpoint (run: .\get-rds-endpoint.ps1)" -ForegroundColor White
Write-Host "  2. Run the SQL script:" -ForegroundColor White
Write-Host "     psql -h YOUR_RDS_ENDPOINT -p 5432 -U postgres -d insuranceloom -f $sqlFile" -ForegroundColor Gray
Write-Host ""
Write-Host "Or use pgAdmin/DBeaver to run the SQL file" -ForegroundColor White

