# Deploy to EC2 - Quick Guide

## Prerequisites
- SSH access to your EC2 instance
- Git repository is up to date (code is pushed to GitHub)

## Deployment Steps

### 1. SSH into EC2 Instance
```bash
ssh ec2-user@your-ec2-ip
# or if using a key file:
ssh -i /path/to/your-key.pem ec2-user@your-ec2-ip
```

### 2. Navigate to Project Directory
```bash
cd /home/ec2-user/Insurance_loom
```

### 3. Pull Latest Code
```bash
git pull origin main
```

### 4. Navigate to API Directory and Publish
```bash
cd InsuranceLoom.Api
dotnet publish -c Release -o /var/www/api
```

### 5. Set Correct Permissions
```bash
sudo chown -R ec2-user:ec2-user /var/www/api
sudo chmod -R 755 /var/www/api
```

### 6. Restart API Service
```bash
sudo systemctl restart insuranceloom-api.service
```

### 7. Check Service Status
```bash
sudo systemctl status insuranceloom-api.service
```

### 8. Check API Logs (if needed)
```bash
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
```

## Verify Deployment

### Test API Locally (on EC2)
```bash
curl http://localhost:5000/api/servicetypes
```

### Test API Publicly
```bash
curl https://api.insuranceloom.com/api/servicetypes
```

## Troubleshooting

If the service fails to start:
1. Check logs: `sudo journalctl -u insuranceloom-api.service -n 100 --no-pager`
2. Verify appsettings.json exists: `ls -la /var/www/api/appsettings.json`
3. Check database connection in appsettings.json
4. Ensure port 5000 is not in use: `sudo ss -tulpn | grep :5000`

## Quick One-Line Deployment (if already set up)
```bash
cd /home/ec2-user/Insurance_loom && git pull origin main && cd InsuranceLoom.Api && dotnet publish -c Release -o /var/www/api && sudo chown -R ec2-user:ec2-user /var/www/api && sudo systemctl restart insuranceloom-api.service && sudo systemctl status insuranceloom-api.service
```

