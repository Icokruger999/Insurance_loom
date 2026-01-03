# EC2 Deployment Instructions

## Quick Deployment

### Option 1: Using the Deployment Script (Recommended)

1. **Connect to your EC2 instance:**
   ```bash
   ssh ec2-user@YOUR_EC2_IP
   # Or use your SSH key path:
   ssh -i /path/to/your-key.pem ec2-user@YOUR_EC2_IP
   ```

2. **Navigate to the API directory:**
   ```bash
   cd /var/www/api
   ```

3. **Pull the latest code:**
   ```bash
   git pull origin main
   ```

4. **Run the deployment script:**
   ```bash
   chmod +x deploy-ec2.sh
   ./deploy-ec2.sh
   ```

   Or if the script is not in the repo directory, copy it first:
   ```bash
   # From your local machine
   scp deploy-ec2.sh ec2-user@YOUR_EC2_IP:/home/ec2-user/
   
   # Then on EC2
   sudo mv /home/ec2-user/deploy-ec2.sh /var/www/api/
   cd /var/www/api
   chmod +x deploy-ec2.sh
   sudo ./deploy-ec2.sh
   ```

### Option 2: Manual Deployment Commands

Run these commands on your EC2 instance:

```bash
# 1. Navigate to API directory
cd /var/www/api

# 2. Pull latest code from GitHub
git pull origin main

# 3. Build the application
dotnet publish -c Release -o ./publish

# 4. Stop the service
sudo systemctl stop insuranceloom-api.service

# 5. Copy published files
sudo cp -r ./publish/* /var/www/api/

# 6. Set proper permissions (adjust user if needed)
sudo chown -R ec2-user:ec2-user /var/www/api

# 7. Start the service
sudo systemctl start insuranceloom-api.service

# 8. Check service status
sudo systemctl status insuranceloom-api.service

# 9. View logs if needed
sudo journalctl -u insuranceloom-api.service -f
```

## Database Migrations

**Important:** The application now runs migrations automatically on startup! The `MigrationRunner` will:
- Execute Entity Framework migrations
- Run all SQL files in the `Data/Migrations` folder (like `005_SeedServiceTypes.sql`)

You don't need to run SQL scripts manually anymore. The migrations will run automatically when the API starts.

However, if you need to run a migration manually, you can:

```bash
# Connect to PostgreSQL
psql -U postgres -d insuranceloom

# Then run the SQL file
\i InsuranceLoom.Api/Data/Migrations/005_SeedServiceTypes.sql
```

Or from command line:
```bash
psql -U postgres -d insuranceloom -f InsuranceLoom.Api/Data/Migrations/005_SeedServiceTypes.sql
```

## Verify Deployment

1. **Check if the API is running:**
   ```bash
   sudo systemctl status insuranceloom-api.service
   ```

2. **Check the API logs:**
   ```bash
   sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
   ```

3. **Test the API endpoint:**
   ```bash
   curl https://api.insuranceloom.com/api/servicetypes
   ```

4. **Check Nginx is working:**
   ```bash
   sudo systemctl status nginx
   ```

## Troubleshooting

### If the service won't start:
```bash
# Check logs for errors
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager

# Check if port is in use
sudo netstat -tulpn | grep :5000

# Verify appsettings.json exists and is valid
cat /var/www/api/appsettings.json | python3 -m json.tool
```

### If migrations fail:
- Check the application logs for migration errors
- Verify database connection string in `appsettings.json`
- Ensure PostgreSQL is running: `sudo systemctl status postgresql`

### If you get permission errors:
```bash
# Fix ownership
sudo chown -R ec2-user:ec2-user /var/www/api

# Fix permissions
sudo chmod -R 755 /var/www/api
```

## Quick Reference Commands

```bash
# Restart API service
sudo systemctl restart insuranceloom-api.service

# View real-time logs
sudo journalctl -u insuranceloom-api.service -f

# View last 100 log lines
sudo journalctl -u insuranceloom-api.service -n 100

# Check service status
sudo systemctl status insuranceloom-api.service

# Stop service
sudo systemctl stop insuranceloom-api.service

# Start service
sudo systemctl start insuranceloom-api.service
```

## After Deployment

1. ✅ Verify the API is running
2. ✅ Check that service types are loaded (automatic migration should have run)
3. ✅ Test the Create New Client form in the Broker Portal
4. ✅ Verify documents can be uploaded
5. ✅ Check that welcome emails are being sent

