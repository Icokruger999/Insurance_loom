# Deployment Summary - Agent/Broker Updates

## Overview

This update changes all user-facing references from "Broker" to "Agent" throughout the application, and adds new API endpoints for agent activity statistics.

## What Changed

### Frontend Files (Deploy to Amplify via GitHub):
- ✅ `index.html` - Login modals updated to "Agent"
- ✅ `broker-portal.html` - Changed to "Agent Portal"
- ✅ `broker-portal.js` - Updated all references
- ✅ `manager-portal.html` - Navigation updated
- ✅ `manager-portal.js` - "Broker Activity" → "Agent Activity"
- ✅ All UI text and labels updated

### Backend Files (Deploy to EC2):
- ✅ `PolicyApprovalController.cs` - New endpoints: `/agents/activity/*`
- ✅ `AuthController.cs` - Email messages updated
- ✅ `PolicyHolderController.cs` - Error messages updated
- ✅ `EmailService.cs` - Email templates updated

### Database Migrations:
- ✅ `011_AddMoreTestPoliciesForBrokerActivity.sql` - Fixed status constraints
- ✅ Address columns added (street_address, city, province, etc.)

## Deployment Steps

### Step 1: Push to GitHub (Frontend Auto-Deploys to Amplify)

```bash
# Commit all changes
git add .
git commit -m "Update: Change Broker to Agent throughout application, add agent activity endpoints"

# Push to main branch
git push origin main
```

**Amplify will automatically:**
- Detect the push to main branch
- Deploy all frontend files
- Update the live site at insuranceloom.com

**Check Amplify Console:**
- Go to: https://console.aws.amazon.com/amplify/
- Monitor the build progress
- Verify deployment completes successfully

### Step 2: Update EC2 Instance (Backend API)

**Option A: Using the Update Script**

1. SSH into EC2:
   ```bash
   ssh -i /path/to/key.pem ec2-user@YOUR_EC2_IP
   ```

2. Run the update script:
   ```bash
   cd /home/ec2-user/Insurance_loom
   git pull origin main
   chmod +x update-ec2-api.sh
   ./update-ec2-api.sh
   ```

**Option B: Manual Update**

```bash
# On EC2 instance
cd /home/ec2-user/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## New API Endpoints

The following endpoints have been updated:

- `GET /api/policy-approval/agents/activity/stats` (was `/brokers/activity/stats`)
- `GET /api/policy-approval/agents/activity/latest-policies` (was `/brokers/activity/latest-policies`)
- `GET /api/policy-approval/agents/activity/performance` (was `/brokers/activity/performance`)
- `GET /api/policy-approval/regions/statistics` (new endpoint)

## Verification Checklist

After deployment, verify:

### Frontend (Amplify):
- [ ] Login modal shows "Agent" instead of "Broker"
- [ ] Agent Portal loads correctly
- [ ] Manager Portal shows "Agent Activity" in navigation
- [ ] Agent Activity Dashboard loads
- [ ] Region statistics chart displays data

### Backend (EC2):
- [ ] API service is running: `sudo systemctl status insuranceloom-api.service`
- [ ] New endpoints respond: `curl https://api.insuranceloom.com/api/policy-approval/agents/activity/stats`
- [ ] Region statistics endpoint works: `curl https://api.insuranceloom.com/api/policy-approval/regions/statistics`
- [ ] Database migrations completed (check logs)
- [ ] No errors in API logs: `sudo journalctl -u insuranceloom-api.service -n 50`

### Integration:
- [ ] Agent login works
- [ ] Manager can view Agent Activity Dashboard
- [ ] Region statistics load in dashboard
- [ ] All API calls from frontend succeed

## Rollback Plan (If Needed)

### Frontend Rollback:
- Go to Amplify Console
- Select previous successful deployment
- Click "Redeploy this version"

### Backend Rollback:
```bash
# On EC2
cd /home/ec2-user/Insurance_loom
git checkout <previous-commit-hash>
cd InsuranceLoom.Api
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo systemctl start insuranceloom-api.service
```

## Important Notes

1. **Database Migrations**: Run automatically on API startup. Check logs if issues occur.
2. **API Endpoints**: Old `/brokers/activity/*` endpoints are replaced with `/agents/activity/*`
3. **Frontend**: All API calls in `manager-portal.js` have been updated to use new endpoints
4. **No Breaking Changes**: Internal code still uses "broker" variables/entities, only UI text changed

## Support

If you encounter issues:
1. Check API logs: `sudo journalctl -u insuranceloom-api.service -f`
2. Check Amplify build logs in AWS Console
3. Verify database connection in `appsettings.json`
4. Test API endpoints directly with curl

