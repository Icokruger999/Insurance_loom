#!/bin/bash

# EC2 Deployment Script for Insurance Loom API
# This script updates the API code on EC2 and restarts the service

set -e  # Exit on error

echo "========================================="
echo "Insurance Loom API - EC2 Deployment"
echo "========================================="
echo ""

# Configuration
API_DIR="/var/www/api"
SERVICE_NAME="insuranceloom-api.service"
GIT_REPO="https://github.com/Icokruger999/Insurance_loom.git"
BRANCH="main"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if running as root or with sudo
if [ "$EUID" -ne 0 ]; then 
    echo -e "${YELLOW}Note: Some commands may require sudo privileges${NC}"
fi

echo "Step 1: Navigate to API directory"
cd "$API_DIR" || {
    echo -e "${RED}Error: Cannot access $API_DIR${NC}"
    echo "Please ensure the directory exists and you have proper permissions"
    exit 1
}

echo -e "${GREEN}✓${NC} Current directory: $(pwd)"
echo ""

echo "Step 2: Pull latest code from GitHub"
git pull origin "$BRANCH" || {
    echo -e "${RED}Error: Failed to pull from GitHub${NC}"
    echo "Please check your internet connection and Git credentials"
    exit 1
}
echo -e "${GREEN}✓${NC} Code updated from GitHub"
echo ""

echo "Step 3: Build the application"
dotnet publish -c Release -o ./publish || {
    echo -e "${RED}Error: Build failed${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Application built successfully"
echo ""

echo "Step 4: Stop the service"
sudo systemctl stop "$SERVICE_NAME" || {
    echo -e "${YELLOW}Warning: Service may not be running${NC}"
}
echo -e "${GREEN}✓${NC} Service stopped"
echo ""

echo "Step 5: Copy published files"
sudo cp -r ./publish/* /var/www/api/ || {
    echo -e "${RED}Error: Failed to copy files${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Files copied"
echo ""

echo "Step 6: Set proper permissions"
sudo chown -R ec2-user:ec2-user /var/www/api || {
    echo -e "${YELLOW}Warning: Could not set ownership (may need to adjust user)${NC}"
}
echo -e "${GREEN}✓${NC} Permissions set"
echo ""

echo "Step 7: Start the service"
sudo systemctl start "$SERVICE_NAME" || {
    echo -e "${RED}Error: Failed to start service${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Service started"
echo ""

echo "Step 8: Check service status"
sleep 2
sudo systemctl status "$SERVICE_NAME" --no-pager -l || {
    echo -e "${YELLOW}Warning: Service status check failed${NC}"
}
echo ""

echo "========================================="
echo -e "${GREEN}Deployment completed successfully!${NC}"
echo "========================================="
echo ""
echo "To view logs, run:"
echo "  sudo journalctl -u $SERVICE_NAME -f"
echo ""
echo "To check service status, run:"
echo "  sudo systemctl status $SERVICE_NAME"
echo ""

