# EC2 Deployment Instructions

## Quick Deployment Script

A deployment script has been created to make EC2 updates easier. Here's how to use it:

### Step 1: Copy the script to EC2

From your local machine, copy the script to EC2:

```bash
scp deploy-ec2.sh ec2-user@YOUR_EC2_IP:/home/ec2-user/
```

Or if you're already on EC2, clone the repo and the script will be there.

### Step 2: Make the script executable

On EC2, run:

```bash
chmod +x deploy-ec2.sh
```

### Step 3: Run the deployment script

```bash
./deploy-ec2.sh
```

Or if you need sudo:

```bash
sudo ./deploy-ec2.sh
```

## What the script does:

1. ✅ Navigates to `/var/www/api`
2. ✅ Pulls latest code from GitHub
3. ✅ Builds the application with `dotnet publish`
4. ✅ Stops the API service
5. ✅ Copies published files
6. ✅ Sets proper permissions
7. ✅ Starts the API service
8. ✅ Checks service status

## Manual Deployment (Alternative)

If you prefer to deploy manually:

```bash
# 1. Navigate to API directory
cd /var/www/api

# 2. Pull latest code
git pull origin main

# 3. Build the application
dotnet publish -c Release -o ./publish

# 4. Stop the service
sudo systemctl stop insuranceloom-api.service

# 5. Copy files
sudo cp -r ./publish/* /var/www/api/

# 6. Start the service
sudo systemctl start insuranceloom-api.service

# 7. Check status
sudo systemctl status insuranceloom-api.service
```

## Troubleshooting

### If the script fails:

1. **Check Git credentials**: Make sure you have access to the repository
2. **Check .NET SDK**: Ensure `dotnet` is installed and in PATH
3. **Check permissions**: You may need to run with `sudo`
4. **Check service name**: Verify the service name is `insuranceloom-api.service`

### View logs:

```bash
sudo journalctl -u insuranceloom-api.service -f
```

### Check service status:

```bash
sudo systemctl status insuranceloom-api.service
```

## Running Database Migrations

After deploying, you may need to run database migrations:

```bash
# Connect to PostgreSQL
psql -U postgres -d insuranceloom

# Run the service types migration
\i InsuranceLoom.Api/Data/Migrations/005_SeedServiceTypes.sql
```

Or copy the SQL file to EC2 and run it:

```bash
psql -U postgres -d insuranceloom -f /path/to/005_SeedServiceTypes.sql
```

