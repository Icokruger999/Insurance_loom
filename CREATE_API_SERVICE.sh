#!/bin/bash

# Script to Create and Setup API Systemd Service
# Run this on your EC2 instance

set -e

echo "========================================="
echo "Creating Insurance Loom API Service"
echo "========================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
SERVICE_FILE="/etc/systemd/system/insuranceloom-api.service"
DEPLOY_DIR="/var/www/api"
DOTNET_PATH=$(which dotnet)

# Check if dotnet is installed
if [ -z "$DOTNET_PATH" ]; then
    echo -e "${RED}✗ .NET SDK not found!${NC}"
    echo "   Please install .NET SDK first"
    echo "   Check: dotnet --version"
    exit 1
fi

echo -e "${GREEN}✓${NC} Found .NET at: $DOTNET_PATH"
echo ""

# Check if API directory exists
if [ ! -d "$DEPLOY_DIR" ]; then
    echo -e "${YELLOW}⚠ API directory not found: $DEPLOY_DIR${NC}"
    echo "   Creating directory..."
    sudo mkdir -p "$DEPLOY_DIR"
    sudo chown -R ec2-user:ec2-user "$DEPLOY_DIR"
    echo -e "${GREEN}✓${NC} Created $DEPLOY_DIR"
    echo ""
    echo -e "${YELLOW}⚠ You need to deploy the API first!${NC}"
    echo "   Run: cd /home/ec2-user/Insurance_loom/InsuranceLoom.Api"
    echo "   Then: dotnet publish -c Release -o /var/www/api"
    echo ""
    read -p "Continue creating service file anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Check if API DLL exists
if [ ! -f "$DEPLOY_DIR/InsuranceLoom.Api.dll" ]; then
    echo -e "${YELLOW}⚠ API DLL not found: $DEPLOY_DIR/InsuranceLoom.Api.dll${NC}"
    echo "   The service will be created but won't start until you deploy the API"
    echo ""
fi

# Create service file
echo "Creating systemd service file..."
sudo tee "$SERVICE_FILE" > /dev/null <<EOF
[Unit]
Description=Insurance Loom API
After=network.target postgresql.service

[Service]
Type=notify
User=ec2-user
Group=ec2-user
WorkingDirectory=$DEPLOY_DIR
ExecStart=$DOTNET_PATH $DEPLOY_DIR/InsuranceLoom.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=insuranceloom-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

# Security settings
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF

echo -e "${GREEN}✓${NC} Service file created at $SERVICE_FILE"
echo ""

# Reload systemd
echo "Reloading systemd daemon..."
sudo systemctl daemon-reload
echo -e "${GREEN}✓${NC} Systemd daemon reloaded"
echo ""

# Enable service (auto-start on boot)
echo "Enabling service (auto-start on boot)..."
sudo systemctl enable insuranceloom-api.service
echo -e "${GREEN}✓${NC} Service enabled"
echo ""

# Check if API is deployed
if [ -f "$DEPLOY_DIR/InsuranceLoom.Api.dll" ]; then
    echo "Starting service..."
    sudo systemctl start insuranceloom-api.service
    sleep 2
    
    # Check status
    if sudo systemctl is-active --quiet insuranceloom-api.service; then
        echo -e "${GREEN}✓${NC} Service started successfully!"
    else
        echo -e "${YELLOW}⚠ Service started but may have errors${NC}"
        echo "   Check status: sudo systemctl status insuranceloom-api.service"
        echo "   Check logs: sudo journalctl -u insuranceloom-api.service -n 50 --no-pager"
    fi
else
    echo -e "${YELLOW}⚠ API not deployed yet - service created but not started${NC}"
    echo "   Deploy the API first, then run:"
    echo "   sudo systemctl start insuranceloom-api.service"
fi

echo ""
echo "========================================="
echo "Service Setup Complete!"
echo "========================================="
echo ""
echo "Useful commands:"
echo "  Status:  sudo systemctl status insuranceloom-api.service"
echo "  Start:    sudo systemctl start insuranceloom-api.service"
echo "  Stop:     sudo systemctl stop insuranceloom-api.service"
echo "  Restart: sudo systemctl restart insuranceloom-api.service"
echo "  Logs:    sudo journalctl -u insuranceloom-api.service -f"
echo ""

