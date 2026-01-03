# Correct EC2 Deployment Steps

The `publish` directory doesn't exist, which means `dotnet publish` either failed or wasn't run. Here's the correct sequence:

## Step-by-Step Commands:

```bash
# 1. Navigate to repository
cd ~/Insurance_loom

# 2. Pull latest code
git pull origin main

# 3. Navigate to API project
cd InsuranceLoom.Api

# 4. Build and publish (this creates the publish directory)
dotnet publish -c Release -o ./publish

# 5. Check if publish directory was created
ls -la ./publish

# 6. If publish exists, stop service
sudo systemctl stop insuranceloom-api.service

# 7. Copy files
sudo cp -r ./publish/* /var/www/api/

# 8. Set permissions
sudo chown -R ec2-user:ec2-user /var/www/api

# 9. Start service
sudo systemctl start insuranceloom-api.service

# 10. Check status
sudo systemctl status insuranceloom-api.service
```

## If dotnet publish fails:

Check for errors:
```bash
dotnet publish -c Release -o ./publish 2>&1 | tee publish-output.log
```

View the log:
```bash
cat publish-output.log
```

## Alternative: Publish directly to /var/www/api

If you want to publish directly:

```bash
cd ~/Insurance_loom/InsuranceLoom.Api
sudo systemctl stop insuranceloom-api.service
dotnet publish -c Release -o /var/www/api
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

