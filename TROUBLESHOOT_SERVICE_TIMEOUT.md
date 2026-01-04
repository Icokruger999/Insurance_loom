# Troubleshoot Service Timeout Error

## Problem
The service `insuranceloom-api.service` is failing to start with a timeout error.

## Immediate Steps to Diagnose

### 1. Check Service Status
```bash
sudo systemctl status insuranceloom-api.service
```

### 2. Check Recent Logs
```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

### 3. Check for Application Errors
```bash
sudo journalctl -xeu insuranceloom-api.service --no-pager | tail -50
```

## Common Causes and Solutions

### Cause 1: Database Connection Timeout
**Symptoms:** Logs show connection timeout or "Unable to connect to database"

**Solution:**
```bash
# Test database connection
psql -U postgres -d insuranceloom -c "SELECT 1;"

# Check connection string in appsettings.json
cat /var/www/api/appsettings.json | grep -A 5 ConnectionStrings

# Verify database is accessible
ping YOUR_RDS_ENDPOINT
```

### Cause 2: Migration Hanging
**Symptoms:** Logs show "Running migrations" but never complete

**Solution:**
```bash
# Check if migrations are running
ps aux | grep dotnet

# Manually test migration connection
cd /var/www/api
dotnet InsuranceLoom.Api.dll --help

# If migrations are the issue, you may need to run them manually or increase timeout
```

### Cause 3: Port Already in Use
**Symptoms:** "Address already in use" error

**Solution:**
```bash
# Check what's using port 5000 or 5001
sudo netstat -tulpn | grep :5000
sudo netstat -tulpn | grep :5001

# Kill the process if needed
sudo kill -9 <PID>
```

### Cause 4: Application Startup Error
**Symptoms:** Application crashes immediately on startup

**Solution:**
```bash
# Try running the application manually to see the error
cd /var/www/api
dotnet InsuranceLoom.Api.dll

# Or if it's a published app:
./InsuranceLoom.Api
```

### Cause 5: Service Configuration Issue
**Symptoms:** Service file has incorrect settings

**Solution:**
```bash
# Check service file
cat /etc/systemd/system/insuranceloom-api.service

# Verify WorkingDirectory and ExecStart are correct
# Increase timeout if needed:
# TimeoutStartSec=300
```

## Quick Fix: Increase Service Timeout

Edit the service file:
```bash
sudo nano /etc/systemd/system/insuranceloom-api.service
```

Add or modify:
```ini
[Service]
TimeoutStartSec=300
TimeoutStopSec=30
```

Then reload and restart:
```bash
sudo systemctl daemon-reload
sudo systemctl restart insuranceloom-api.service
```

## Manual Startup Test

To see the exact error, try running the application manually:

```bash
cd /var/www/api
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://localhost:5000
dotnet InsuranceLoom.Api.dll
```

This will show you the exact error message that's preventing startup.

## Check Application Logs

```bash
# Check if there's a log file
ls -la /var/www/api/logs/
ls -la /var/log/insuranceloom/

# Check system logs
sudo dmesg | tail -20
```

## Verify Dependencies

```bash
# Check .NET runtime
dotnet --version

# Check if all required files exist
ls -la /var/www/api/

# Verify permissions
sudo chown -R ec2-user:ec2-user /var/www/api
sudo chmod +x /var/www/api/InsuranceLoom.Api
```

## Most Likely Issue

Given the timeout, it's most likely:
1. **Database connection hanging** - Check RDS security groups and connection string
2. **Migration taking too long** - Check if migrations are running on a large database
3. **Application waiting for a resource** - Check logs for what it's waiting for

Run the diagnostic commands above to identify the exact issue.

