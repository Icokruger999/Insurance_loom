# EC2 Deployment Instructions

This guide will help you deploy the updated Insurance Loom API to your EC2 instance.

## Prerequisites

- SSH access to your EC2 instance
- Git repository is set up on EC2
- .NET SDK installed on EC2
- Systemd service configured

## Option 1: Using the Deployment Script (Recommended)

1. **SSH into your EC2 instance:**
   ```bash
   ssh -i /path/to/your-key.pem ec2-user@your-ec2-ip-or-domain
   ```

2. **Navigate to the repository directory:**
   ```bash
   cd ~/Insurance_loom
   ```

3. **Make the deployment script executable (if not already):**
   ```bash
   chmod +x deploy-ec2.sh
   ```

4. **Run the deployment script:**
   ```bash
   ./deploy-ec2.sh
   ```

   The script will:
   - Pull the latest code from GitHub
   - Build and publish the API
   - Stop the service
   - Copy files to deployment directory
   - Restart the service
   - Show service status

## Option 2: Manual Deployment Steps

If you prefer to run the commands manually:

1. **SSH into your EC2 instance:**
   ```bash
   ssh -i /path/to/your-key.pem ec2-user@your-ec2-ip-or-domain
   ```

2. **Navigate to the repository:**
   ```bash
   cd ~/Insurance_loom
   ```

3. **Pull the latest code from GitHub:**
   ```bash
   git pull origin main
   ```

4. **Navigate to the API project directory:**
   ```bash
   cd InsuranceLoom.Api
   ```

5. **Stop the API service:**
   ```bash
   sudo systemctl stop insuranceloom-api.service
   ```

6. **Build and publish the API:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

7. **Copy published files to deployment directory:**
   ```bash
   sudo cp -r ./publish/* /var/www/api/
   ```

8. **Set correct permissions:**
   ```bash
   sudo chown -R ec2-user:ec2-user /var/www/api
   ```

9. **Start the API service:**
   ```bash
   sudo systemctl start insuranceloom-api.service
   ```

10. **Check service status:**
    ```bash
    sudo systemctl status insuranceloom-api.service
    ```

11. **View logs (if needed):**
    ```bash
    sudo journalctl -u insuranceloom-api.service -f --no-pager
    ```

## Verify Deployment

1. **Test the API endpoint:**
   ```bash
   curl https://api.insuranceloom.com/api/servicetypes
   ```

2. **Check if the new endpoint is available:**
   ```bash
   curl -X POST https://api.insuranceloom.com/api/application/email-pdf \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -d '{"formData":{},"recipients":[]}'
   ```

## Troubleshooting

### Service won't start:
```bash
# Check service logs
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager

# Check if port is already in use
sudo netstat -tulpn | grep :5000

# Restart the service
sudo systemctl restart insuranceloom-api.service
```

### Build errors:
```bash
# Clean and rebuild
cd InsuranceLoom.Api
dotnet clean
dotnet restore
dotnet build -c Release
```

### Permission errors:
```bash
# Fix permissions
sudo chown -R ec2-user:ec2-user /var/www/api
sudo chmod -R 755 /var/www/api
```

### Database migration issues:
The application should automatically run migrations on startup. If you see database errors, check the logs:
```bash
sudo journalctl -u insuranceloom-api.service -f
```

## Important Notes

- The deployment directory is: `/var/www/api`
- The service name is: `insuranceloom-api.service`
- Make sure your `appsettings.json` in `/var/www/api/` has the correct configuration
- The API should automatically apply database migrations on startup
- All new endpoints require authentication (Bearer token)

## Quick Deployment Command (All-in-one)

If you want to run everything in one command:

```bash
cd ~/Insurance_loom && git pull origin main && cd InsuranceLoom.Api && sudo systemctl stop insuranceloom-api.service && dotnet publish -c Release -o ./publish && sudo cp -r ./publish/* /var/www/api/ && sudo chown -R ec2-user:ec2-user /var/www/api && sudo systemctl start insuranceloom-api.service && sudo systemctl status insuranceloom-api.service --no-pager
```

