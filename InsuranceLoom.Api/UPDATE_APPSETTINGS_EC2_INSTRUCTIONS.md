# Update appsettings.json on EC2

## Quick Update Instructions

**SSH into your EC2 instance, then run these commands:**

```bash
# 1. Create a backup
sudo cp /var/www/api/appsettings.json /var/www/api/appsettings.json.backup.$(date +%Y%m%d_%H%M%S)

# 2. Update the file
sudo nano /var/www/api/appsettings.json
```

**Copy the JSON content from `FINAL_APPSETTINGS_EC2.md` and replace the entire file contents.**

**Important Notes:**
- ❌ **NO `BrokerApproval` section** - it's been removed and is no longer needed
- ✅ Make sure all values from `FINAL_APPSETTINGS_EC2.md` are copied correctly

**After saving the file:**

```bash
# 3. Validate JSON syntax
cat /var/www/api/appsettings.json | python3 -m json.tool

# 4. Set correct permissions
sudo chown www-data:www-data /var/www/api/appsettings.json
sudo chmod 644 /var/www/api/appsettings.json

# 5. Restart the API service
sudo systemctl restart insuranceloom-api.service

# 6. Check if service started successfully
sudo systemctl status insuranceloom-api.service
```

## Verify Update

```bash
# Check the service is running
sudo systemctl is-active insuranceloom-api.service

# Check logs for any errors
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager

# Test the API endpoint
curl http://localhost:5000/api/company
```

