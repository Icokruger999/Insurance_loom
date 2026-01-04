# Troubleshoot EC2 Service Crash

## The Fix Has Been Pushed

The compilation error has been fixed and pushed to GitHub. The issue was `policyIds` being used before definition.

## Steps to Fix on EC2

### 1. Pull Latest Code
```bash
cd /home/ec2-user/Insurance_loom
git pull origin main
```

### 2. Check Service Logs (to see exact error)
```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

### 3. Try Building Locally First
```bash
cd InsuranceLoom.Api
dotnet build
```

If build fails, you'll see the exact error.

### 4. Check Database Connection
```bash
# Test PostgreSQL connection
psql -U postgres -d insuranceloom -c "SELECT COUNT(*) FROM policies;"
```

### 5. Check appsettings.json
```bash
cat /var/www/api/appsettings.json | grep -A 5 ConnectionStrings
```

Make sure the connection string is correct.

### 6. Try Running Manually (to see startup errors)
```bash
cd /var/www/api
dotnet InsuranceLoom.Api.dll
```

This will show you the exact error message.

### 7. Check for Missing Dependencies
```bash
# Check if .NET runtime is installed
dotnet --version

# Check if all required packages are available
cd /home/ec2-user/Insurance_loom/InsuranceLoom.Api
dotnet restore
```

### 8. Rebuild and Deploy
```bash
cd /home/ec2-user/Insurance_loom/InsuranceLoom.Api
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
```

### 9. Monitor Logs in Real-Time
```bash
sudo journalctl -u insuranceloom-api.service -f
```

## Common Issues

### Issue: Database Connection Failed
**Solution:** Check `appsettings.json` connection string

### Issue: Migration Error
**Solution:** Check migration logs, may need to run migrations manually

### Issue: Missing .NET Runtime
**Solution:** Install .NET runtime:
```bash
sudo yum install dotnet-runtime-8.0
```

### Issue: Port Already in Use
**Solution:** 
```bash
sudo netstat -tulpn | grep :5000
sudo kill <PID>
```

### Issue: Permission Denied
**Solution:**
```bash
sudo chown -R ec2-user:ec2-user /var/www/api
sudo chmod -R 755 /var/www/api
```

## Quick Fix Script

Run this on EC2:

```bash
cd /home/ec2-user/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
dotnet build
if [ $? -eq 0 ]; then
    dotnet publish -c Release -o ./publish
    sudo systemctl stop insuranceloom-api.service
    sudo cp -r ./publish/* /var/www/api/
    sudo chown -R ec2-user:ec2-user /var/www/api
    sudo systemctl start insuranceloom-api.service
    sleep 3
    sudo systemctl status insuranceloom-api.service
else
    echo "Build failed - check errors above"
fi
```

