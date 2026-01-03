# Easy EC2 Deployment Guide

## Quick Deployment

After the initial setup, deploying updates to EC2 is now as simple as:

### Option 1: Using the Deployment Script (Recommended)

```bash
# SSH into EC2
ssh -i your-key.pem ec2-user@34.246.222.13

# Make the script executable (first time only)
chmod +x ~/Insurance_loom/InsuranceLoom.Api/deploy.sh

# Run the deployment script
~/Insurance_loom/InsuranceLoom.Api/deploy.sh
```

That's it! The script will:
1. Pull latest code from GitHub
2. Build the API
3. Publish to `/var/www/api`
4. Restart the service
5. Show service status

### Option 2: Manual Deployment (if you prefer)

```bash
cd ~/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
dotnet publish -c Release -o /var/www/api
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## First Time Setup on EC2

If this is your first time, you need to:

1. **Clone the repository:**
   ```bash
   cd ~
   git clone https://github.com/Icokruger999/Insurance_loom.git
   ```

2. **Make deployment script executable:**
   ```bash
   chmod +x ~/Insurance_loom/InsuranceLoom.Api/deploy.sh
   ```

3. **Run deployment:**
   ```bash
   ~/Insurance_loom/InsuranceLoom.Api/deploy.sh
   ```

## Troubleshooting

### Check Service Logs
```bash
sudo journalctl -u insuranceloom-api.service -f
```

### Check Service Status
```bash
sudo systemctl status insuranceloom-api.service
```

### Restart Service Manually
```bash
sudo systemctl restart insuranceloom-api.service
```

### Test API
```bash
curl https://api.insuranceloom.com/api/auth/broker/login
```

## Notes

- The deployment script uses `sudo` only when necessary (for systemctl and file permissions)
- Make sure your `appsettings.json` in `/var/www/api` has the correct settings
- The script will stop if any step fails (set -e)
- Service logs are available via `journalctl` if you need to debug issues

