#!/bin/bash

# Insurance Loom API Deployment Script
# Run this script on your EC2 instance after SSH connection

set -e  # Exit on error

echo "========================================"
echo "Insurance Loom API Deployment"
echo "========================================"
echo ""

# Configuration - UPDATE THESE VALUES
GITHUB_REPO="https://github.com/Icokruger999/Insurance_loom.git"
API_DIR="/var/www/api"
SERVICE_NAME="insuranceloom-api"
EC2_USER="ec2-user"  # Change to "ubuntu" if using Ubuntu

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Configuration:${NC}"
echo "  Repository: $GITHUB_REPO"
echo "  API Directory: $API_DIR"
echo "  Service Name: $SERVICE_NAME"
echo ""
read -p "Continue? (y/n) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Deployment cancelled."
    exit 1
fi

# Step 1: Create directory
echo ""
echo -e "${GREEN}Step 1: Creating directory...${NC}"
sudo mkdir -p $API_DIR
sudo chown -R $EC2_USER:$EC2_USER $API_DIR
echo "✅ Directory created"

# Step 2: Clone repository
echo ""
echo -e "${GREEN}Step 2: Cloning repository...${NC}"
cd /var/www
if [ -d "Insurance_loom" ]; then
    echo "Repository already exists, pulling latest changes..."
    cd Insurance_loom
    git pull
else
    echo "Cloning repository..."
    git clone $GITHUB_REPO
    cd Insurance_loom
fi
echo "✅ Repository cloned/updated"

# Step 3: Build and publish
echo ""
echo -e "${GREEN}Step 3: Building and publishing API...${NC}"
cd InsuranceLoom.Api
dotnet publish -c Release -o $API_DIR
sudo chown -R $EC2_USER:$EC2_USER $API_DIR
echo "✅ API built and published"

# Step 4: Check if appsettings.json exists
echo ""
echo -e "${GREEN}Step 4: Checking configuration...${NC}"
if [ ! -f "$API_DIR/appsettings.json" ]; then
    echo -e "${RED}ERROR: appsettings.json not found!${NC}"
    exit 1
fi
echo "✅ Configuration file found"

# Step 5: Configure Nginx
echo ""
echo -e "${GREEN}Step 5: Configuring Nginx...${NC}"
NGINX_CONF="/etc/nginx/conf.d/api.conf"

# Get EC2 public IP
EC2_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)

sudo tee $NGINX_CONF > /dev/null <<EOF
server {
    listen 80;
    server_name api.insuranceloom.com $EC2_IP;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

# Test Nginx configuration
if sudo nginx -t; then
    sudo systemctl reload nginx
    echo "✅ Nginx configured and reloaded"
else
    echo -e "${RED}ERROR: Nginx configuration test failed!${NC}"
    exit 1
fi

# Step 6: Create systemd service
echo ""
echo -e "${GREEN}Step 6: Creating systemd service...${NC}"
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"

sudo tee $SERVICE_FILE > /dev/null <<EOF
[Unit]
Description=Insurance Loom API
After=network.target

[Service]
Type=notify
User=$EC2_USER
WorkingDirectory=$API_DIR
ExecStart=/usr/bin/dotnet $API_DIR/InsuranceLoom.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$SERVICE_NAME
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd and enable service
sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
echo "✅ Systemd service created"

# Step 7: Start the service
echo ""
echo -e "${GREEN}Step 7: Starting API service...${NC}"
sudo systemctl start $SERVICE_NAME
sleep 3

# Check status
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    echo "✅ API service is running"
else
    echo -e "${RED}ERROR: API service failed to start!${NC}"
    echo "Check logs with: sudo journalctl -u $SERVICE_NAME -n 50"
    exit 1
fi

# Step 8: Test API
echo ""
echo -e "${GREEN}Step 8: Testing API...${NC}"
sleep 2
if curl -s http://localhost:5000/api/auth/broker/login > /dev/null; then
    echo "✅ API is responding"
else
    echo -e "${YELLOW}⚠️  API test returned error (this might be normal - endpoint needs POST data)${NC}"
fi

# Summary
echo ""
echo "========================================"
echo -e "${GREEN}✅ Deployment Complete!${NC}"
echo "========================================"
echo ""
echo "API Status:"
sudo systemctl status $SERVICE_NAME --no-pager -l | head -n 5
echo ""
echo "Useful commands:"
echo "  Check status:  sudo systemctl status $SERVICE_NAME"
echo "  View logs:     sudo journalctl -u $SERVICE_NAME -f"
echo "  Restart API:   sudo systemctl restart $SERVICE_NAME"
echo "  Test API:      curl http://localhost:5000/api/auth/broker/login"
echo ""
echo "⚠️  IMPORTANT: Update appsettings.json with your RDS connection string!"
echo "   sudo nano $API_DIR/appsettings.json"
echo ""
echo "Your API should be accessible at:"
echo "  http://$EC2_IP/api"
echo ""

