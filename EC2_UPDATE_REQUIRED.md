# EC2 Update Required - Database Query Endpoints

## Issue
The dashboard shows 404 errors because the EC2 API hasn't been updated with the new endpoints that query the database.

## Endpoints That Need to Be Deployed

All these endpoints **query the database directly** using Entity Framework:

### 1. Agent Activity Stats
- **Route:** `GET /api/policy-approval/agents/activity/stats`
- **Queries:** 
  - `policies` table (counts by status)
  - `policy_approvals` table (review counts)
- **Returns:** Active, expired, pending counts, review stats

### 2. Latest Policies
- **Route:** `GET /api/policy-approval/agents/activity/latest-policies`
- **Queries:** 
  - `policies` table (with joins to `brokers` and `service_types`)
- **Returns:** Latest 10 policies with broker and service type info

### 3. Agent Performance
- **Route:** `GET /api/policy-approval/agents/activity/performance`
- **Queries:** 
  - `policies` table (grouped by broker)
- **Returns:** Top 10 agents by premium, with policy counts

### 4. Region Statistics
- **Route:** `GET /api/policy-approval/regions/statistics`
- **Queries:** 
  - `policies` table (joined with `policy_holders`)
  - Groups by `province` and `city` from `policy_holders`
- **Returns:** Policy counts and premiums by region

## All Endpoints Query Database Tables

✅ **Policies Table** - Main policy data
✅ **Policy_Approvals Table** - Approval status and review data
✅ **Brokers Table** - Agent information
✅ **Policy_Holders Table** - Address and region data
✅ **Service_Types Table** - Service type information

## Update EC2 Now

Run on your EC2 instance:

```bash
cd /home/ec2-user/Insurance_loom
git pull origin main
chmod +x update-ec2-api.sh
./update-ec2-api.sh
```

Or manually:

```bash
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

## Verify Database Has Data

After updating, if dashboard still shows no data, check if database has policies:

```bash
# On EC2, connect to database
psql -U postgres -d insuranceloom

# Check if policies exist
SELECT COUNT(*) FROM policies;

# Check if brokers exist
SELECT COUNT(*) FROM brokers;

# If no data, run test data migrations
\i InsuranceLoom.Api/Data/Migrations/010_AddTestBrokers.sql
\i InsuranceLoom.Api/Data/Migrations/011_AddMoreTestPoliciesForBrokerActivity.sql
```

## Expected Behavior After Update

1. ✅ 404 errors should disappear
2. ✅ Dashboard should show data from database
3. ✅ All statistics should reflect actual database records
4. ✅ Region statistics should show data grouped by province/city
