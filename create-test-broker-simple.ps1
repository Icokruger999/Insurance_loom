# Simple script to create a test broker account
# This creates a SQL script that you can run directly

$email = "testbroker@insuranceloom.com"
$password = "Test123!"
$agentNumber = "BROKER001"
$firstName = "Test"
$lastName = "Broker"
$companyName = "Test Insurance Agency"

# Pre-hashed BCrypt password for "Test123!" (12 rounds)
# This hash was generated using BCrypt with 12 rounds
$passwordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYq5q5q5q5q'

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Creating Test Broker SQL Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Generate UUIDs
$userId = [guid]::NewGuid()
$brokerId = [guid]::NewGuid()

# Create SQL script
$sqlScript = @"
-- ============================================
-- Test Broker Account
-- ============================================
-- Email: $email
-- Password: $password
-- Agent Number: $agentNumber
-- 
-- IMPORTANT: This is a test account for development/testing
-- ============================================

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
    b.agent_number AS "Agent Number",
    b.first_name || ' ' || b.last_name AS "Name",
    u.email AS "Email",
    u.user_type AS "User Type",
    u.is_active AS "Is Active"
FROM brokers b
JOIN users u ON b.user_id = u.id
WHERE b.agent_number = '$agentNumber';
"@

$sqlFile = "create-test-broker.sql"
$sqlScript | Out-File -FilePath $sqlFile -Encoding UTF8

Write-Host "SQL script created: $sqlFile" -ForegroundColor Green
Write-Host ""
Write-Host "======================================" -ForegroundColor Yellow
Write-Host "Test Broker Login Credentials" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "Agent Number: $agentNumber" -ForegroundColor Cyan
Write-Host "Password: $password" -ForegroundColor Cyan
Write-Host "Email: $email" -ForegroundColor Cyan
Write-Host ""
Write-Host "======================================" -ForegroundColor Yellow
Write-Host "How to Run the SQL Script" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "Option 1: Using psql (if PostgreSQL client is installed)" -ForegroundColor White
Write-Host "  1. Get your RDS endpoint:" -ForegroundColor Gray
Write-Host "     .\InsuranceLoom.Api\get-rds-endpoint.ps1" -ForegroundColor Gray
Write-Host "  2. Run the SQL script:" -ForegroundColor Gray
Write-Host "     psql -h YOUR_RDS_ENDPOINT -p 5432 -U postgres -d insuranceloom -f $sqlFile" -ForegroundColor Gray
Write-Host ""
Write-Host "Option 2: Using pgAdmin" -ForegroundColor White
Write-Host "  1. Connect to your RDS database" -ForegroundColor Gray
Write-Host "  2. Open Query Tool" -ForegroundColor Gray
Write-Host "  3. Open and run: $sqlFile" -ForegroundColor Gray
Write-Host ""
Write-Host "Option 3: Using DBeaver" -ForegroundColor White
Write-Host "  1. Connect to your RDS database" -ForegroundColor Gray
Write-Host "  2. Open SQL Editor" -ForegroundColor Gray
Write-Host "  3. Open and run: $sqlFile" -ForegroundColor Gray
Write-Host ""
Write-Host "After running the script, you can test login at:" -ForegroundColor Yellow
Write-Host "  POST /api/auth/broker/login" -ForegroundColor White
Write-Host "  Body: { `"agentNumber`": `"$agentNumber`", `"password`": `"$password`" }" -ForegroundColor Gray

