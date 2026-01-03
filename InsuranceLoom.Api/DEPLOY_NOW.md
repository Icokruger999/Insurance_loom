# Deploy to EC2 - Complete Steps

## Step 1: SSH into EC2
```bash
ssh -i your-key.pem ec2-user@34.246.222.13
```

## Step 2: Pull Latest Code and Deploy
Run these commands on EC2:

```bash
cd ~/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
chmod +x deploy.sh
./deploy.sh
```

## Step 3: Verify Deployment
```bash
sudo systemctl status insuranceloom-api.service
```

## Step 4: Test Company Endpoint
```bash
curl https://api.insuranceloom.com/api/company
```

You should see JSON with "Astutetech Data" and "Pogo Group".

## Step 5: Check Logs (if needed)
```bash
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
```

## What Gets Deployed:
- ✅ Updated broker registration (strict company validation)
- ✅ Company lookup only (no broker creation)
- ✅ Company API endpoints (managers only can create)
- ✅ Updated frontend (removed company creation checkbox)
- ✅ Fixed compilation errors

---

**One-liner deployment command:**
```bash
cd ~/Insurance_loom && git pull origin main && cd InsuranceLoom.Api && ./deploy.sh
```

