# Push Changes to GitHub - Quick Guide

## Step 1: Commit All Changes Locally

Run these commands in your local repository:

```bash
# Add all changed files
git add .

# Commit with descriptive message
git commit -m "Update: Change Broker to Agent throughout application, add agent activity endpoints and update script"

# Push to GitHub
git push origin main
```

## Step 2: Update EC2 Instance

After pushing to GitHub, SSH into your EC2 and run:

```bash
# Navigate to repository
cd /home/ec2-user/Insurance_loom

# Pull latest code (this will get the update script)
git pull origin main

# Make script executable
chmod +x update-ec2-api.sh

# Run the update script
./update-ec2-api.sh
```

## What Gets Pushed

### Frontend Files (Auto-deploys to Amplify):
- ✅ index.html
- ✅ broker-portal.html/js
- ✅ manager-portal.html/js
- ✅ All UI text changes

### Backend Files (Deploy to EC2):
- ✅ PolicyApprovalController.cs
- ✅ AuthController.cs
- ✅ PolicyHolderController.cs
- ✅ EmailService.cs
- ✅ update-ec2-api.sh (new script)

### Database Migrations:
- ✅ 011_AddMoreTestPoliciesForBrokerActivity.sql
- ✅ 009_AddTestPoliciesData.sql (updated)

## After Push

1. **Amplify** will automatically deploy frontend changes (takes 2-3 minutes)
2. **EC2** needs manual update using the script above

## Verify Deployment

### Check Amplify:
- Go to: https://console.aws.amazon.com/amplify/
- Check build status
- Verify deployment completes

### Check EC2:
```bash
# On EC2
sudo systemctl status insuranceloom-api.service
curl http://localhost:5000/api/servicetypes
```

