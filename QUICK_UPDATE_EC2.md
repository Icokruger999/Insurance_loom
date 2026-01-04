# Quick EC2 Update Guide

## Update Existing EC2 Instance

### Option 1: Using the Update Script (Recommended)

1. **SSH into your EC2 instance:**
   ```bash
   ssh -i /path/to/your-key.pem ec2-user@YOUR_EC2_IP
   ```

2. **Download the update script:**
   ```bash
   cd /home/ec2-user
   wget https://raw.githubusercontent.com/Icokruger999/Insurance_loom/main/update-ec2-api.sh
   # Or if you already have the repo:
   cd /home/ec2-user/Insurance_loom
   git pull origin main
   ```

3. **Make it executable and run:**
   ```bash
   chmod +x update-ec2-api.sh
   ./update-ec2-api.sh
   ```

### Option 2: Manual Update Commands

Run these commands on your EC2 instance:

```bash
# 1. Navigate to repository
cd /home/ec2-user/Insurance_loom

# 2. Pull latest code
git pull origin main

# 3. Navigate to API project
cd InsuranceLoom.Api

# 4. Build the application
dotnet publish -c Release -o ./publish

# 5. Stop the service
sudo systemctl stop insuranceloom-api.service

# 6. Copy files
sudo cp -r ./publish/* /var/www/api/

# 7. Set permissions
sudo chown -R ec2-user:ec2-user /var/www/api

# 8. Start the service
sudo systemctl start insuranceloom-api.service

# 9. Check status
sudo systemctl status insuranceloom-api.service
```

## What Gets Updated

### Backend (EC2):
- ✅ API code changes (Agent/Broker terminology updates)
- ✅ New API endpoints (`/agents/activity/*`)
- ✅ Controller updates
- ✅ Email service updates
- ✅ Database migrations (run automatically on startup)

### Frontend (Amplify):
- ✅ Login modals (Agent instead of Broker)
- ✅ Manager Portal (Agent Activity Dashboard)
- ✅ Agent Portal updates
- ✅ All UI text changes

**Note:** Frontend changes deploy automatically to Amplify when you push to GitHub main branch.

## Verify Updates

### Check API:
```bash
# On EC2
curl http://localhost:5000/api/servicetypes

# From your machine
curl https://api.insuranceloom.com/api/servicetypes
```

### Check Service Status:
```bash
sudo systemctl status insuranceloom-api.service
```

### View Logs:
```bash
sudo journalctl -u insuranceloom-api.service -f
```

## Troubleshooting

### If service won't start:
```bash
# Check logs
sudo journalctl -u insuranceloom-api.service -n 100

# Check if port is in use
sudo netstat -tulpn | grep :5000

# Verify appsettings.json
cat /var/www/api/appsettings.json
```

### If migrations fail:
- Check application logs for migration errors
- Verify database connection string
- Ensure PostgreSQL is running: `sudo systemctl status postgresql`

### Rollback (if needed):
```bash
# Stop service
sudo systemctl stop insuranceloom-api.service

# Restore from backup (if created)
sudo cp -r /var/www/api_backup_* /var/www/api

# Start service
sudo systemctl start insuranceloom-api.service
```

## After Update Checklist

- [ ] API service is running
- [ ] API endpoints respond correctly
- [ ] Database migrations completed
- [ ] Frontend deployed to Amplify (check Amplify console)
- [ ] Test login with Agent credentials
- [ ] Verify Agent Activity Dashboard loads
- [ ] Check that region statistics endpoint works

