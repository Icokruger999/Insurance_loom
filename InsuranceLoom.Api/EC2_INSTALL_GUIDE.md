# EC2 Installation Guide - What to Install After Creating Instance

This guide tells you exactly what to install on your EC2 instance once you've created it through the AWS Console.

## Prerequisites

- ✅ EC2 instance created in AWS Console
- ✅ Security group configured (SSH port 22, HTTP 80, HTTPS 443)
- ✅ Key pair downloaded (.pem file)
- ✅ Instance is running and you have the Public IP address

## Step 1: Connect to Your EC2 Instance

### Using PowerShell or Git Bash:

```bash
# Navigate to folder with your .pem key file
cd C:\path\to\your\keys

# Connect (replace with your actual values)
ssh -i "your-key-name.pem" ec2-user@YOUR_EC2_PUBLIC_IP

# For Ubuntu instances, use:
# ssh -i "your-key-name.pem" ubuntu@YOUR_EC2_PUBLIC_IP
```

**First time connection:** Type `yes` when prompted about host authenticity.

---

## Step 2: Update System

### For Amazon Linux 2023:
```bash
sudo dnf update -y
```

### For Ubuntu 22.04:
```bash
sudo apt update && sudo apt upgrade -y
```

---

## Step 3: Install .NET 8 SDK (Required for Building)

**Note:** We need the SDK (not just runtime) to build the API on EC2.

### For Amazon Linux 2023:
```bash
# Install .NET 8 SDK
sudo dnf install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
# Should show: 8.0.x
```

### For Amazon Linux 2:
```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/amazonlinux/2/packages-microsoft-prod.rpm

# Install .NET 8 SDK
sudo yum install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
# Should show: 8.0.x
```

### For Ubuntu 22.04:
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update

# Install .NET 8 SDK
sudo apt install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
# Should show: 8.0.x
```

---

## Step 4: Install Nginx (Reverse Proxy)

Nginx will handle HTTPS and forward requests to your API.

### For Amazon Linux 2023:
```bash
sudo dnf install -y nginx
```

### For Ubuntu 22.04:
```bash
sudo apt install -y nginx
```

### Start and Enable Nginx:
```bash
sudo systemctl start nginx
sudo systemctl enable nginx

# Check status
sudo systemctl status nginx
```

### Verify Nginx is running:
Open in browser: `http://YOUR_EC2_IP` - You should see the default Nginx page.

---

## Step 5: Install Git (for deploying from repository)

### For Amazon Linux 2023:
```bash
sudo dnf install -y git
```

### For Ubuntu 22.04:
```bash
sudo apt install -y git
```

### Verify:
```bash
git --version
```

---

## Step 6: Install Certbot (for SSL Certificates)

This is needed for HTTPS/SSL setup later.

### For Amazon Linux 2023:
```bash
sudo dnf install -y certbot python3-certbot-nginx
```

### For Ubuntu 22.04:
```bash
sudo apt install -y certbot python3-certbot-nginx
```

### Verify:
```bash
certbot --version
```

---

## Step 7: Configure Nginx for Your API

Create the Nginx configuration file:

```bash
sudo nano /etc/nginx/conf.d/api.conf
```

Paste this configuration (replace `YOUR_EC2_IP` with your actual IP):

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

Save and exit: `Ctrl+X`, then `Y`, then `Enter`

Test Nginx configuration:
```bash
sudo nginx -t
```

If test passes, reload Nginx:
```bash
sudo systemctl reload nginx
```

---

## Step 8: Create Directory for Your API

```bash
# Create directory for your API
sudo mkdir -p /var/www/api
sudo chown -R ec2-user:ec2-user /var/www/api

# For Ubuntu, use:
# sudo chown -R ubuntu:ubuntu /var/www/api
```

---

## Step 9: Deploy Your API

### Option A: Deploy from Git Repository

```bash
# Clone your repository
cd /var/www
sudo git clone https://github.com/YOUR_USERNAME/Insurance_loom.git
cd Insurance_loom/InsuranceLoom.Api

# Build and publish
sudo dotnet publish -c Release -o /var/www/api

# Set ownership
sudo chown -R ec2-user:ec2-user /var/www/api
```

### Option B: Upload Files Manually (from your local machine)

From your local PowerShell:

```powershell
# Build your API locally
cd InsuranceLoom.Api
dotnet publish -c Release -o ./publish

# Upload to EC2 (replace with your values)
scp -i "your-key-name.pem" -r ./publish/* ec2-user@YOUR_EC2_IP:/home/ec2-user/api/

# Then on EC2, move files:
# sudo cp -r /home/ec2-user/api/* /var/www/api/
# sudo chown -R ec2-user:ec2-user /var/www/api
```

---

## Step 10: Configure appsettings.json

Edit the configuration file:

```bash
sudo nano /var/www/api/appsettings.json
```

Update these settings:
- **ConnectionStrings**: Use your RDS endpoint
- **JwtSettings**: Update secret key for production
- **AWS**: Your AWS credentials (already configured)
- **Environment**: Set `ASPNETCORE_ENVIRONMENT=Production`

---

## Step 11: Create Systemd Service

Create a service file to run your API:

```bash
sudo nano /etc/systemd/system/insuranceloom-api.service
```

Paste this content:

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

**For Ubuntu, change `User=ec2-user` to `User=ubuntu`**

Save and exit, then:

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (starts on boot)
sudo systemctl enable insuranceloom-api

# Start service
sudo systemctl start insuranceloom-api

# Check status
sudo systemctl status insuranceloom-api
```

---

## Step 12: Set Up SSL Certificate (Optional but Recommended)

This requires your domain (`api.insuranceloom.com`) to point to your EC2 IP first.

### Configure DNS in Namecheap:
- Host: `api`
- Type: `A Record`
- Value: `YOUR_EC2_IP`
- TTL: Automatic

Wait 5-15 minutes for DNS propagation, then:

```bash
# Get SSL certificate
sudo certbot --nginx -d api.insuranceloom.com

# Follow the prompts:
# - Enter email address
# - Agree to terms
# - Choose whether to redirect HTTP to HTTPS (recommended: Yes)
```

Certbot will automatically configure Nginx for HTTPS.

---

## Step 13: Test Your API

1. **Test HTTP:**
   ```
   http://YOUR_EC2_IP/api/auth/broker/login
   ```

2. **Test HTTPS (if SSL configured):**
   ```
   https://api.insuranceloom.com/api/auth/broker/login
   ```

3. **Check API logs:**
   ```bash
   sudo journalctl -u insuranceloom-api -f
   ```

---

## Useful Commands

### Check API Status:
```bash
sudo systemctl status insuranceloom-api
```

### Restart API:
```bash
sudo systemctl restart insuranceloom-api
```

### View API Logs:
```bash
sudo journalctl -u insuranceloom-api -f
```

### View Nginx Logs:
```bash
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log
```

### Test API Directly (bypass nginx):
```bash
curl http://localhost:5000/api/auth/broker/login
```

### Restart Nginx:
```bash
sudo systemctl restart nginx
```

---

## Security Checklist

After setup, remember to:

- ✅ Restrict SSH (port 22) to your IP only in Security Group
- ✅ Remove port 5000 from Security Group (after nginx is working)
- ✅ Use SSL/HTTPS (set up with Certbot)
- ✅ Keep system updated: `sudo dnf update` or `sudo apt update`
- ✅ Store secrets securely (use environment variables or AWS Secrets Manager)

---

## Troubleshooting

### API won't start:
```bash
# Check logs
sudo journalctl -u insuranceloom-api -n 50

# Check if port 5000 is in use
sudo netstat -tlnp | grep 5000
```

### Nginx not working:
```bash
# Check nginx status
sudo systemctl status nginx

# Test configuration
sudo nginx -t

# Check error logs
sudo tail -f /var/log/nginx/error.log
```

### Can't connect to API:
1. Check security group allows port 80/443
2. Check API is running: `sudo systemctl status insuranceloom-api`
3. Check nginx is running: `sudo systemctl status nginx`
4. Test locally: `curl http://localhost:5000/api/auth/broker/login`

---

## Summary of What Gets Installed

1. ✅ .NET 8 Runtime - Runs your API
2. ✅ Nginx - Reverse proxy for HTTPS
3. ✅ Git - For cloning repository
4. ✅ Certbot - For SSL certificates
5. ✅ Systemd Service - Keeps API running

That's everything you need! After these steps, your API will be running and accessible.

