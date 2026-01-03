#!/bin/bash

# EC2 Deployment Script for Insurance Loom API
# This script updates the API code on EC2 and restarts the service

set -e  # Exit on error

echo "========================================="
echo "Insurance Loom API - EC2 Deployment"
echo "========================================="
echo ""

# Configuration
REPO_DIR="/home/ec2-user/Insurance_loom"
API_PROJECT_DIR="$REPO_DIR/InsuranceLoom.Api"
DEPLOY_DIR="/var/www/api"
SERVICE_NAME="insuranceloom-api.service"
BRANCH="main"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "Step 1: Navigate to repository directory"
if [ ! -d "$REPO_DIR" ]; then
    echo -e "${YELLOW}Repository not found. Cloning...${NC}"
    cd /home/ec2-user
    git clone https://github.com/Icokruger999/Insurance_loom.git
fi

cd "$REPO_DIR" || {
    echo -e "${RED}Error: Cannot access $REPO_DIR${NC}"
    exit 1
}

echo -e "${GREEN}✓${NC} Current directory: $(pwd)"
echo ""

echo "Step 2: Pull latest code from GitHub"
git pull origin "$BRANCH" || {
    echo -e "${RED}Error: Failed to pull from GitHub${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Code updated from GitHub"
echo ""

echo "Step 3: Navigate to API project directory"
cd "$API_PROJECT_DIR" || {
    echo -e "${RED}Error: Cannot access $API_PROJECT_DIR${NC}"
    exit 1
}

echo "Step 4: Build the application"
dotnet publish -c Release -o ./publish || {
    echo -e "${RED}Error: Build failed${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Application built successfully"
echo ""

echo "Step 5: Stop the service"
sudo systemctl stop "$SERVICE_NAME" || {
    echo -e "${YELLOW}Warning: Service may not be running${NC}"
}
echo -e "${GREEN}✓${NC} Service stopped"
echo ""

echo "Step 6: Copy published files"
sudo cp -r ./publish/* "$DEPLOY_DIR/" || {
    echo -e "${RED}Error: Failed to copy files${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Files copied"
echo ""

echo "Step 7: Set proper permissions"
sudo chown -R ec2-user:ec2-user "$DEPLOY_DIR" || {
    echo -e "${YELLOW}Warning: Could not set ownership${NC}"
}
echo -e "${GREEN}✓${NC} Permissions set"
echo ""

echo "Step 8: Start the service"
sudo systemctl start "$SERVICE_NAME" || {
    echo -e "${RED}Error: Failed to start service${NC}"
    exit 1
}
echo -e "${GREEN}✓${NC} Service started"
echo ""

echo "Step 9: Check service status"
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
