#!/bin/bash
# Complete deployment commands for Company feature

# 1. Navigate to project directory
cd ~/Insurance_loom

# 2. Pull latest code (including the fix)
git pull origin main

# 3. Navigate to API directory
cd InsuranceLoom.Api

# 4. Make deploy script executable (if not already)
chmod +x deploy.sh

# 5. Run deployment script
./deploy.sh

# 6. Check service status
echo "Checking service status..."
sudo systemctl status insuranceloom-api.service --no-pager

# 7. Test company endpoint
echo ""
echo "Testing company endpoint..."
curl -s https://api.insuranceloom.com/api/company | head -20

echo ""
echo "Deployment complete!"

