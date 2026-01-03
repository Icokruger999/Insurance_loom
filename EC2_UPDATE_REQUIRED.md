# EC2 Update Required

## Summary
Yes, you need to update EC2. The following changes require deployment:

### Database Schema Changes:
1. **New `dependents` table** - For people covered under policies (children, spouse, etc.)
2. **Updated `beneficiaries` table** - Added `is_primary` column
3. **Updated `policy_holders` table** - Already has all new fields (migration 006)

### Code Changes:
1. New `Dependent` entity
2. Updated `Beneficiary` entity (added `IsPrimary` field)
3. Updated `ApplicationDbContext` with new configurations
4. New SQL migration: `008_AddDependentsTableAndUpdateBeneficiaries.sql`

## Deployment Steps

### Option 1: Using Deployment Script (Recommended)

```bash
cd /home/ec2-user/Insurance_loom
./deploy-ec2.sh
```

### Option 2: Manual Deployment

```bash
cd /home/ec2-user/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
sudo systemctl stop insuranceloom-api.service
dotnet publish -c Release -o ./publish
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## Automatic Migration Execution

When the API starts, it will **automatically** run the new SQL migration:
- `008_AddDependentsTableAndUpdateBeneficiaries.sql`

This migration will:
- Create the `dependents` table
- Add `is_primary` column to `beneficiaries` table

**No manual SQL execution required** - the MigrationRunner handles this automatically.

## Verification

After deployment, check the logs to confirm migrations ran:

```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager | grep -i migration
```

You should see:
```
Executed migration: 008_AddDependentsTableAndUpdateBeneficiaries.sql
```

## Important Notes

- The migrations use `IF NOT EXISTS` clauses, so they're safe to run multiple times
- If a migration fails, check the logs for details
- The API will continue to run even if migrations fail (errors are logged)
- All database changes are backward compatible (new tables/columns only)

