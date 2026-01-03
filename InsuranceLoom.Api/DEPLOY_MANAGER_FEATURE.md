# Deploy Manager-Company Feature

## Database Setup First

**1. Run Migration Script:**
```bash
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom -f InsuranceLoom.Api/Data/Migrations/003_AddManagerCompanyAndInitialManager.sql
```

This will:
- Add `company_id` column to `managers` table
- Insert Ico Kruger manager (ico@astutetech.co.za) for Astutetech Data company

## Deploy Code

**2. SSH into EC2 and deploy:**
```bash
cd ~/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
./deploy.sh
```

## Verify

**3. Check manager was created:**
```sql
SELECT m.*, c.name as company_name 
FROM managers m 
LEFT JOIN companies c ON m.company_id = c.id 
WHERE m.email = 'ico@astutetech.co.za';
```

**4. Test broker registration:**
- Company: Astutetech Data
- Manager Email: ico@astutetech.co.za
- Approval email should go to ico@astutetech.co.za

