# Setup API Service on EC2 - Quick Guide

## The script isn't on EC2 yet - you need to pull it first!

### Step 1: Pull Latest Code from GitHub

```bash
cd /home/ec2-user/Insurance_loom
git pull origin main
```

### Step 2: Run the Setup Script

```bash
chmod +x CREATE_API_SERVICE.sh
./CREATE_API_SERVICE.sh
```

---

## OR: Create Service File Manually (Faster)

If you want to set it up immediately without pulling code:

### Step 1: Create the Service File

```bash
sudo nano /etc/systemd/system/insuranceloom-api.service
```

### Step 2: Paste This Content

```ini
[Unit]
Description=Insurance Loom API
After=network.target postgresql.service

[Service]
Type=notify
User=ec2-user
Group=ec2-user
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

**Save and exit:** Press `Ctrl+X`, then `Y`, then `Enter`

### Step 3: Reload and Start Service

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (auto-start on boot)
sudo systemctl enable insuranceloom-api.service

# Start the service
sudo systemctl start insuranceloom-api.service

# Check status
sudo systemctl status insuranceloom-api.service
```

---

## Important: Deploy API First!

Before the service can start, make sure the API is deployed:

```bash
cd /home/ec2-user/Insurance_loom/InsuranceLoom.Api
dotnet publish -c Release -o /var/www/api
sudo chown -R ec2-user:ec2-user /var/www/api
```

Then start the service:
```bash
sudo systemctl start insuranceloom-api.service
```

---

## Verify Everything Works

```bash
# Check service status
sudo systemctl status insuranceloom-api.service

# Test API locally
curl http://localhost:5000/api/servicetypes

# Check logs
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
```

