# EC2 Setup Instructions - First Time Setup

## Situation
The `/var/www/api` directory is not a Git repository. You need to either:
1. Clone the repository to a different location, build, then copy files
2. OR initialize git in `/var/www/api` if it's empty

## Option 1: Clone to separate directory (Recommended)

This keeps the source code separate from the deployed files:

```bash
# 1. Create a workspace directory
cd ~
mkdir -p workspace
cd workspace

# 2. Clone the repository (if not already cloned)
git clone https://github.com/Icokruger999/Insurance_loom.git
cd Insurance_loom

# 3. Navigate to the API project
cd InsuranceLoom.Api

# 4. Build the application
dotnet publish -c Release -o ./publish

# 5. Stop the service
sudo systemctl stop insuranceloom-api.service

# 6. Copy published files to /var/www/api
sudo cp -r ./publish/* /var/www/api/

# 7. Set permissions
sudo chown -R ec2-user:ec2-user /var/www/api

# 8. Copy appsettings.json if it doesn't exist (DON'T overwrite if it exists!)
# sudo cp appsettings.json /var/www/api/appsettings.json

# 9. Start the service
sudo systemctl start insuranceloom-api.service

# 10. Check status
sudo systemctl status insuranceloom-api.service
```

## Option 2: Initialize Git in /var/www/api (If directory is empty)

Only use this if `/var/www/api` is empty or you want to manage it as a git repo:

```bash
# 1. Backup existing files if any
sudo cp -r /var/www/api /var/www/api.backup

# 2. Remove existing files (be careful!)
sudo rm -rf /var/www/api/*

# 3. Initialize git repository
cd /var/www/api
sudo git init
sudo git remote add origin https://github.com/Icokruger999/Insurance_loom.git
sudo git fetch origin
sudo git checkout -b main origin/main

# 4. Build
cd InsuranceLoom.Api
sudo dotnet publish -c Release -o ./publish

# 5. Copy published files
sudo cp -r ./publish/* /var/www/api/

# 6. Set permissions
sudo chown -R ec2-user:ec2-user /var/www/api

# 7. Start service
sudo systemctl start insuranceloom-api.service
```

## For Future Deployments (After Initial Setup)

Once you've set up Option 1, future deployments are easier:

```bash
# 1. Navigate to workspace
cd ~/workspace/Insurance_loom/InsuranceLoom.Api

# 2. Pull latest code
git pull origin main

# 3. Build
dotnet publish -c Release -o ./publish

# 4. Stop service
sudo systemctl stop insuranceloom-api.service

# 5. Copy files
sudo cp -r ./publish/* /var/www/api/

# 6. Start service
sudo systemctl start insuranceloom-api.service

# 7. Check status
sudo systemctl status insuranceloom-api.service
```

## Quick Setup Script (Option 1)

Save this as `setup-ec2.sh` and run it:

```bash
#!/bin/bash
cd ~
mkdir -p workspace
cd workspace
git clone https://github.com/Icokruger999/Insurance_loom.git
cd Insurance_loom/InsuranceLoom.Api
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

Run it with:
```bash
chmod +x setup-ec2.sh
./setup-ec2.sh
```

## Important Notes

1. **appsettings.json**: Make sure `/var/www/api/appsettings.json` has your correct database and email credentials. Don't overwrite it when copying files.

2. **Database Migrations**: Run automatically on startup, so no manual SQL scripts needed.

3. **Check Logs**: Always check logs after deployment:
   ```bash
   sudo journalctl -u insuranceloom-api.service -f
   ```

