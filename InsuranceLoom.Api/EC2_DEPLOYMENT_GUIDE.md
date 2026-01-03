# EC2 Deployment Guide for Insurance Loom API

This guide will help you deploy your .NET Core API to an EC2 instance on AWS.

## Prerequisites

- AWS account with credentials configured
- RDS database already set up (you have this)
- Domain `api.insuranceloom.com` configured (optional but recommended)

## Step 1: Launch EC2 Instance

### Option A: Using AWS Console

1. **Go to EC2 Console**
   - Visit: https://console.aws.amazon.com/ec2/
   - Select region: `af-south-1` (Cape Town - same as your RDS)

2. **Launch Instance**
   - Click "Launch Instance"
   - Name: `insuranceloom-api`

3. **Choose AMI**
   - Select: **Amazon Linux 2023** (free tier eligible)
   - Or: **Ubuntu Server 22.04 LTS**

4. **Instance Type**
   - For testing: `t2.micro` (free tier)
   - For production: `t3.small` or `t3.medium`

5. **Key Pair**
   - Create new key pair or use existing
   - Download the `.pem` file (you'll need this to SSH)

6. **Network Settings**
   - Security Group: Create new or use existing
   - Configure security group (see Step 2)

7. **Storage**
   - Default 8 GB is fine for free tier
   - Increase if needed

8. **Launch Instance**

### Option B: Using PowerShell Script

Run the provided PowerShell script:
```powershell
.\InsuranceLoom.Api\setup-ec2-instance.ps1
```

## Step 2: Configure Security Group

Your EC2 instance needs to allow:

1. **SSH (Port 22)** - From your IP only
   - Type: SSH
   - Port: 22
   - Source: My IP (or your IP address)

2. **HTTP (Port 80)** - From anywhere
   - Type: HTTP
   - Port: 80
   - Source: 0.0.0.0/0

3. **HTTPS (Port 443)** - From anywhere
   - Type: HTTPS
   - Port: 443
   - Source: 0.0.0.0/0

4. **Custom TCP (Port 5000)** - From anywhere (temporary, for testing)
   - Type: Custom TCP
   - Port: 5000
   - Source: 0.0.0.0/0
   - Note: Remove this after setting up nginx reverse proxy

## Step 3: Connect to EC2 Instance

### Get Your Instance IP

1. In EC2 Console, find your instance
2. Copy the **Public IPv4 address** (e.g., `13.244.xxx.xxx`)

### SSH Connection (Windows)

Using PowerShell or Git Bash:

```powershell
# Navigate to folder with your .pem key
cd C:\path\to\your\keys

# Set permissions (Git Bash) or use PowerShell
ssh -i "your-key.pem" ec2-user@YOUR_EC2_IP
# For Ubuntu, use: ubuntu@YOUR_EC2_IP
```

**First time connection:**
- Type `yes` when prompted about host authenticity

## Step 4: Install .NET Runtime on EC2

### For Amazon Linux 2023:

```bash
# Update system
sudo dnf update -y

# Install .NET 8 Runtime
sudo dnf install -y dotnet-runtime-8.0

# Verify installation
dotnet --version
```

### For Ubuntu 22.04:

```bash
# Update system
sudo apt update
sudo apt upgrade -y

# Install .NET 8 Runtime
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0

# Verify installation
dotnet --version
```

## Step 5: Install and Configure Nginx (Reverse Proxy)

Nginx will handle HTTPS and forward requests to your API.

### Install Nginx

**Amazon Linux 2023:**
```bash
sudo dnf install -y nginx
```

**Ubuntu:**
```bash
sudo apt install -y nginx
```

### Start Nginx

```bash
sudo systemctl start nginx
sudo systemctl enable nginx
```

### Configure Nginx for API

Create configuration file:

```bash
sudo nano /etc/nginx/conf.d/api.conf
```

Add this configuration:

```nginx
server {
    listen 80;
    server_name api.insuranceloom.com YOUR_EC2_IP;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Test and reload nginx:

```bash
sudo nginx -t
sudo systemctl reload nginx
```

## Step 6: Set Up SSL Certificate (Let's Encrypt)

Install Certbot:

**Amazon Linux 2023:**
```bash
sudo dnf install -y certbot python3-certbot-nginx
```

**Ubuntu:**
```bash
sudo apt install -y certbot python3-certbot-nginx
```

Get SSL certificate (requires domain to point to EC2):

```bash
sudo certbot --nginx -d api.insuranceloom.com
```

Follow the prompts. Certbot will automatically configure nginx for HTTPS.

## Step 7: Deploy Your API

### Option A: Manual Deployment (Quick Test)

1. **Copy files to EC2:**

From your local machine:

```powershell
# Build your API
cd InsuranceLoom.Api
dotnet publish -c Release -o ./publish

# Copy to EC2 (using SCP)
scp -i "your-key.pem" -r ./publish/* ec2-user@YOUR_EC2_IP:/home/ec2-user/api/
```

2. **On EC2, set up the application:**

```bash
# Create app directory
sudo mkdir -p /var/www/api
sudo cp -r /home/ec2-user/api/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
```

### Option B: Using Git (Recommended)

On EC2:

```bash
# Install Git
sudo dnf install -y git  # Amazon Linux
# or
sudo apt install -y git  # Ubuntu

# Clone your repository
cd /var/www
sudo git clone https://github.com/YOUR_USERNAME/Insurance_loom.git
cd Insurance_loom/InsuranceLoom.Api

# Build and publish
sudo dotnet publish -c Release -o /var/www/api
sudo chown -R ec2-user:ec2-user /var/www/api
```

## Step 8: Configure appsettings.json

On EC2:

```bash
sudo nano /var/www/api/appsettings.json
```

Update:
- Connection string (use your RDS endpoint)
- JWT settings
- AWS settings
- Other configuration

## Step 9: Create Systemd Service

Create a service file to run your API:

```bash
sudo nano /etc/systemd/system/insuranceloom-api.service
```

Add this content:

```ini
[Unit]
Description=Insurance Loom API
After=network.target

[Service]
Type=notify
User=ec2-user
WorkingDirectory=/var/www/api
ExecStart=/usr/bin/dotnet /var/www/api/InsuranceLoom.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=insuranceloom-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

Start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable insuranceloom-api
sudo systemctl start insuranceloom-api
sudo systemctl status insuranceloom-api
```

## Step 10: Configure DNS (if using domain)

In Namecheap (or your DNS provider):

1. Add A record:
   - Host: `api`
   - Type: `A Record`
   - Value: `YOUR_EC2_IP`
   - TTL: Automatic

Wait 5-15 minutes for DNS propagation.

## Step 11: Test Your API

1. **Test HTTP:**
   ```
   http://YOUR_EC2_IP/api/auth/broker/login
   ```

2. **Test HTTPS (if SSL configured):**
   ```
   https://api.insuranceloom.com/api/auth/broker/login
   ```

3. **Test from browser:**
   - Visit: `https://api.insuranceloom.com/swagger` (if in development mode)
   - Or test the login endpoint

## Step 12: Update CORS in API

Update `Program.cs` to allow your frontend domain:

```csharp
policy.WithOrigins(
    "https://insuranceloom.com",
    "https://www.insuranceloom.com"
)
```

## Troubleshooting

### Check API logs:
```bash
sudo journalctl -u insuranceloom-api -f
```

### Check Nginx logs:
```bash
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log
```

### Restart services:
```bash
sudo systemctl restart insuranceloom-api
sudo systemctl restart nginx
```

### Test API directly (bypass nginx):
```bash
curl http://localhost:5000/api/auth/broker/login
```

## Security Best Practices

1. ✅ **Remove port 5000 from security group** after nginx is set up
2. ✅ **Keep SSH access restricted** to your IP only
3. ✅ **Use SSL/TLS** (Let's Encrypt is free)
4. ✅ **Keep system updated:** `sudo dnf update` or `sudo apt update`
5. ✅ **Configure firewall:** Use AWS Security Groups properly
6. ✅ **Store secrets securely:** Use AWS Secrets Manager or environment variables
7. ✅ **Set up monitoring:** CloudWatch logs and alarms

## Next Steps

- Set up automated deployments (CI/CD)
- Configure CloudWatch monitoring
- Set up automated backups
- Configure auto-scaling (if needed)
- Set up load balancer (if needed)

## Cost Estimate

- **t2.micro (free tier):** $0/month (first 12 months, then ~$10/month)
- **t3.small:** ~$15-20/month
- **Data transfer:** Varies (usually minimal)
- **Storage (EBS):** ~$1/month for 8GB

**Total:** ~$10-25/month depending on instance size

