#!/bin/bash

# Insurance Loom API Deployment Script
# Usage: ./deploy.sh

set -e  # Exit on error

echo "=========================================="
echo "Insurance Loom API Deployment"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="$HOME/Insurance_loom"
API_DIR="$PROJECT_DIR/InsuranceLoom.Api"
DEPLOY_DIR="/var/www/api"
SERVICE_NAME="insuranceloom-api.service"

# Check if running as root (we'll use sudo for specific commands)
if [ "$EUID" -eq 0 ]; then 
   echo -e "${RED}Please don't run this script as root. It will use sudo when needed.${NC}"
   exit 1
fi

echo -e "${YELLOW}Step 1: Navigating to project directory...${NC}"
cd "$PROJECT_DIR" || { echo -e "${RED}Error: Project directory not found at $PROJECT_DIR${NC}"; exit 1; }

echo -e "${YELLOW}Step 2: Pulling latest code from GitHub...${NC}"
git pull origin main || { echo -e "${RED}Error: Failed to pull from GitHub${NC}"; exit 1; }

echo -e "${YELLOW}Step 3: Navigating to API directory...${NC}"
cd "$API_DIR" || { echo -e "${RED}Error: API directory not found${NC}"; exit 1; }

echo -e "${YELLOW}Step 4: Building and publishing API...${NC}"
dotnet publish -c Release -o "$DEPLOY_DIR" || { 
    echo -e "${RED}Error: Failed to build/publish API${NC}"; 
    exit 1; 
}

echo -e "${YELLOW}Step 5: Setting correct permissions...${NC}"
sudo chown -R ec2-user:ec2-user "$DEPLOY_DIR" || { 
    echo -e "${YELLOW}Warning: Could not change ownership (may not be necessary)${NC}"; 
}

echo -e "${YELLOW}Step 6: Restarting API service...${NC}"
sudo systemctl restart "$SERVICE_NAME" || { 
    echo -e "${RED}Error: Failed to restart service${NC}"; 
    exit 1; 
}

echo -e "${YELLOW}Step 7: Checking service status...${NC}"
sleep 2  # Give service a moment to start
sudo systemctl status "$SERVICE_NAME" --no-pager -l

echo ""
echo -e "${GREEN}=========================================="
echo -e "Deployment Complete!${NC}"
echo -e "${GREEN}=========================================="
echo ""
echo -e "Service Status: $(sudo systemctl is-active $SERVICE_NAME)"
echo ""
echo -e "${YELLOW}To view logs, run:${NC}"
echo "  sudo journalctl -u $SERVICE_NAME -f"
echo ""
echo -e "${YELLOW}To test the API, run:${NC}"
echo "  curl https://api.insuranceloom.com/api/auth/broker/login"
echo ""

