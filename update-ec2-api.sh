#!/bin/bash

# EC2 API Update Script for Insurance Loom
# This script updates the existing EC2 instance with latest code changes
# Run this script on your EC2 instance via SSH

set -e  # Exit on error

echo "========================================="
echo "Insurance Loom API - EC2 Update"
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
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored messages
print_step() {
    echo -e "${BLUE}▶${NC} $1"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

# Check if running as ec2-user or with sudo
if [ "$EUID" -eq 0 ]; then
    print_warning "Running as root. Some commands may need adjustment."
fi

print_step "Step 1: Navigate to repository directory"
if [ ! -d "$REPO_DIR" ]; then
    print_warning "Repository not found at $REPO_DIR"
    print_step "Cloning repository..."
    cd /home/ec2-user
    git clone https://github.com/Icokruger999/Insurance_loom.git || {
        print_error "Failed to clone repository"
        exit 1
    }
    print_success "Repository cloned"
else
    cd "$REPO_DIR" || {
        print_error "Cannot access $REPO_DIR"
        exit 1
    }
    print_success "Repository directory found"
fi

echo ""

print_step "Step 2: Pull latest code from GitHub"
git fetch origin || {
    print_error "Failed to fetch from GitHub"
    exit 1
}

git pull origin "$BRANCH" || {
    print_error "Failed to pull from GitHub"
    exit 1
}

# Show what changed
echo ""
print_step "Recent commits:"
git log --oneline -5
echo ""

print_success "Code updated from GitHub"
echo ""

print_step "Step 3: Navigate to API project directory"
cd "$API_PROJECT_DIR" || {
    print_error "Cannot access $API_PROJECT_DIR"
    exit 1
}
print_success "In API project directory: $(pwd)"
echo ""

print_step "Step 4: Build the application"
echo "Building in Release mode..."
dotnet restore || {
    print_warning "Restore had issues, continuing..."
}

dotnet publish -c Release -o ./publish || {
    print_error "Build failed"
    echo "Check the error messages above"
    exit 1
}
print_success "Application built successfully"
echo ""

print_step "Step 5: Stop the service"
if sudo systemctl is-active --quiet "$SERVICE_NAME"; then
    sudo systemctl stop "$SERVICE_NAME" || {
        print_error "Failed to stop service"
        exit 1
    }
    print_success "Service stopped"
else
    print_warning "Service was not running"
fi
echo ""

print_step "Step 6: Backup current deployment (optional)"
BACKUP_DIR="${DEPLOY_DIR}_backup_$(date +%Y%m%d_%H%M%S)"
if [ -d "$DEPLOY_DIR" ] && [ "$(ls -A $DEPLOY_DIR)" ]; then
    sudo cp -r "$DEPLOY_DIR" "$BACKUP_DIR" 2>/dev/null || {
        print_warning "Could not create backup (this is okay)"
    }
    if [ -d "$BACKUP_DIR" ]; then
        print_success "Backup created at $BACKUP_DIR"
    fi
fi
echo ""

print_step "Step 7: Copy published files"
sudo cp -r ./publish/* "$DEPLOY_DIR/" || {
    print_error "Failed to copy files"
    exit 1
}
print_success "Files copied to $DEPLOY_DIR"
echo ""

print_step "Step 8: Preserve appsettings.json"
# Don't overwrite appsettings.json if it exists
if [ -f "$DEPLOY_DIR/appsettings.json" ]; then
    print_success "appsettings.json preserved (not overwritten)"
else
    print_warning "appsettings.json not found in deployment directory"
    print_warning "You may need to copy it manually"
fi
echo ""

print_step "Step 9: Set proper permissions"
sudo chown -R ec2-user:ec2-user "$DEPLOY_DIR" || {
    print_warning "Could not set ownership (may need to check permissions)"
}
sudo chmod -R 755 "$DEPLOY_DIR" || {
    print_warning "Could not set permissions"
}
print_success "Permissions set"
echo ""

print_step "Step 10: Start the service"
sudo systemctl start "$SERVICE_NAME" || {
    print_error "Failed to start service"
    echo ""
    echo "Check logs with: sudo journalctl -u $SERVICE_NAME -n 50"
    exit 1
}
print_success "Service started"
echo ""

print_step "Step 11: Wait for service to initialize"
sleep 5
echo ""

print_step "Step 12: Check service status"
if sudo systemctl is-active --quiet "$SERVICE_NAME"; then
    print_success "Service is running"
    echo ""
    sudo systemctl status "$SERVICE_NAME" --no-pager -l | head -n 10
else
    print_error "Service is not running!"
    echo ""
    echo "Recent logs:"
    sudo journalctl -u "$SERVICE_NAME" -n 30 --no-pager
    exit 1
fi
echo ""

print_step "Step 13: Test API endpoint"
sleep 2
if curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/servicetypes | grep -q "200\|401\|403"; then
    print_success "API is responding"
else
    print_warning "API test returned unexpected response (may need authentication)"
fi
echo ""

echo "========================================="
echo -e "${GREEN}✓ Update completed successfully!${NC}"
echo "========================================="
echo ""
echo "Service Information:"
echo "  Service Name: $SERVICE_NAME"
echo "  Deployment Dir: $DEPLOY_DIR"
echo "  Status: $(sudo systemctl is-active $SERVICE_NAME)"
echo ""
echo "Useful Commands:"
echo "  View logs:        sudo journalctl -u $SERVICE_NAME -f"
echo "  View last 50:     sudo journalctl -u $SERVICE_NAME -n 50"
echo "  Restart service:  sudo systemctl restart $SERVICE_NAME"
echo "  Check status:     sudo systemctl status $SERVICE_NAME"
echo ""
echo "API Endpoints:"
echo "  Local:            http://localhost:5000/api"
echo "  Public:           https://api.insuranceloom.com/api"
echo ""
echo -e "${YELLOW}Note:${NC} Frontend changes will be deployed automatically to Amplify"
echo "      when you push to GitHub main branch."
echo ""

