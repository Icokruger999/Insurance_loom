# EC2 To-Do Checklist

## ✅ Already Done:
- [x] Updated email password to `Stacey@1122` in appsettings.json

## 📋 What to Do Next:

### 1. Restart API Service (for email password change)
```bash
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

### 2. Run Database Migration (Manager-Company Association)
```bash
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom -f ~/Insurance_loom/InsuranceLoom.Api/Data/Migrations/003_AddManagerCompanyAndInitialManager.sql
```

This will:
- Add `company_id` column to `managers` table
- Create initial manager: Ico Kruger (ico@astutetech.co.za) for Astutetech Data

### 3. Deploy Latest Code Changes
```bash
cd ~/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
./deploy.sh
```

This deploys:
- Manager-company association
- Manager email validation in broker registration
- Updated approval workflow (uses manager email)

### 4. Verify Everything Works

**Check API is running:**
```bash
sudo systemctl status insuranceloom-api.service
```

**Verify manager was created:**
```bash
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom -c "SELECT m.email, m.first_name, m.last_name, c.name as company_name FROM managers m LEFT JOIN companies c ON m.company_id = c.id WHERE m.email = 'ico@astutetech.co.za';"
```

**Test email:**
```bash
curl -X POST https://api.insuranceloom.com/api/test/email \
  -H "Content-Type: application/json" \
  -d '{"to": "ico@astutetech.co.za", "subject": "Test Email", "body": "Testing email functionality"}'
```

**Test company endpoint:**
```bash
curl https://api.insuranceloom.com/api/company
```

## Summary of Changes:
1. ✅ Email password updated
2. ⏳ Restart API service
3. ⏳ Run database migration (003)
4. ⏳ Deploy latest code
5. ⏳ Test and verify

