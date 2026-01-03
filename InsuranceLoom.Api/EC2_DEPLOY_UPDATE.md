# Deploy API Updates to EC2

This guide helps you deploy the latest API changes (including broker registration email notifications) to your EC2 instance.

## Steps to Deploy

### 1. SSH into EC2 Instance
```bash
ssh -i your-key.pem ec2-user@34.246.222.13
```

### 2. Navigate to Project Directory
```bash
cd ~/Insurance_loom
```

### 3. Pull Latest Code
```bash
git pull origin main
```

### 4. Build and Publish the API
```bash
cd InsuranceLoom.Api
dotnet publish -c Release -o /var/www/api
```

### 5. Restart the API Service
```bash
sudo systemctl restart insuranceloom-api.service
```

### 6. Check Service Status
```bash
sudo systemctl status insuranceloom-api.service
```

### 7. Check Logs (if needed)
```bash
sudo journalctl -u insuranceloom-api.service -f
```

## Verify Deployment

### Test the API
```bash
curl http://localhost:5000/api/health
```

Or test broker registration endpoint:
```bash
curl -X POST http://localhost:5000/api/auth/broker/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

## Notes

- The email service uses the settings from `appsettings.json` on EC2
- Make sure email credentials are configured correctly in `/var/www/api/appsettings.json`
- If emails fail to send, check the API logs for error messages
- Email failures won't prevent broker registration (errors are caught and logged)

