# Debug API Not Listening on Port 5000

If the service shows "active (running)" but curl fails, the app is likely crashing on startup.

## Step 1: Check Logs
```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

Look for:
- Database connection errors
- Configuration errors
- Missing dependencies
- Port binding errors

## Step 2: Check if Process is Actually Running
```bash
ps aux | grep dotnet
ps aux | grep InsuranceLoom.Api
```

## Step 3: Check Port Binding
```bash
sudo netstat -tlnp | grep 5000
sudo ss -tlnp | grep 5000
```

If nothing is listening on 5000, the app crashed.

## Step 4: Check Service File Configuration
```bash
cat /etc/systemd/system/insuranceloom-api.service
```

Should have:
- `Type=simple`
- `ExecStart=/usr/bin/dotnet /var/www/api/InsuranceLoom.Api.dll`
- `Environment=ASPNETCORE_URLS=http://localhost:5000`

## Step 5: Test Running the DLL Directly
```bash
cd /var/www/api
/usr/bin/dotnet InsuranceLoom.Api.dll
```

This will show startup errors directly in the terminal.

## Step 6: Common Issues and Fixes

### Database Connection Failed
- Check `/var/www/api/appsettings.json` connection string
- Verify RDS is accessible from EC2
- Test: `psql -h YOUR_RDS_ENDPOINT -U postgres -d insuranceloom`

### Missing Dependencies
- Ensure .NET runtime is installed: `dotnet --version`
- Check all DLLs are in `/var/www/api`

### Configuration Errors
- Verify `appsettings.json` has all required settings
- Check JWT secret key is set
- Verify email settings if using email service

### Port Already in Use
```bash
sudo lsof -i :5000
# If something else is using it, kill it or change port
```

## Step 7: Run with More Verbose Logging

Temporarily modify service file to see more output:
```bash
sudo nano /etc/systemd/system/insuranceloom-api.service
```

Add to [Service] section:
```
Environment=ASPNETCORE_ENVIRONMENT=Development
StandardOutput=journal
StandardError=journal
```

Then:
```bash
sudo systemctl daemon-reload
sudo systemctl restart insuranceloom-api.service
sudo journalctl -u insuranceloom-api.service -f
```

