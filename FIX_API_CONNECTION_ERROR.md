# Fix API Connection Error - Step-by-Step Guide

## Error Message
"Connection error. Please make sure the API is running on https://api.insuranceloom.com/api"

## Troubleshooting Steps (Run on EC2)

### Step 1: Check if API Service is Running
```bash
sudo systemctl status insuranceloom-api.service
```

**Expected:** Service should show as `active (running)`

**If not running, start it:**
```bash
sudo systemctl start insuranceloom-api.service
sudo systemctl enable insuranceloom-api.service  # Enable auto-start
sudo systemctl status insuranceloom-api.service
```

### Step 2: Check API Logs for Errors
```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

**Look for:**
- Database connection errors
- Port binding errors
- Configuration errors
- Migration errors

### Step 3: Test API Locally on EC2
```bash
curl http://localhost:5000/api/servicetypes
```

**Expected:** Should return JSON data or at least a response (not connection refused)

**If this fails:**
- The API is not running or crashed
- Check logs from Step 2

### Step 4: Check if Port 5000 is Listening
```bash
sudo netstat -tulpn | grep :5000
# or
sudo ss -tulpn | grep :5000
```

**Expected:** Should show a process listening on port 5000

**If nothing shows:**
- API is not running
- Check service status and logs

### Step 5: Check Nginx Status
```bash
sudo systemctl status nginx
```

**Expected:** Nginx should be `active (running)`

**If not running:**
```bash
sudo systemctl start nginx
sudo systemctl enable nginx
```

### Step 6: Check Nginx Configuration for API
```bash
sudo cat /etc/nginx/sites-available/api.insuranceloom.com
# or
sudo cat /etc/nginx/conf.d/api.insuranceloom.com.conf
```

**Should contain:**
- `proxy_pass http://localhost:5000;`
- SSL certificate configuration
- Proper server_name

### Step 7: Test Nginx Configuration
```bash
sudo nginx -t
```

**Expected:** Should say "syntax is ok" and "test is successful"

**If errors:**
- Fix the configuration file
- Restart nginx: `sudo systemctl restart nginx`

### Step 8: Test Public API Endpoint
```bash
curl https://api.insuranceloom.com/api/servicetypes
```

**Expected:** Should return JSON data

**If this fails but localhost works:**
- Nginx configuration issue
- SSL certificate issue
- DNS issue

### Step 9: Check Security Groups (AWS Console)
1. Go to EC2 Console
2. Select your instance
3. Check Security Groups
4. Ensure:
   - Port 443 (HTTPS) is open to 0.0.0.0/0
   - Port 80 (HTTP) is open (optional, for redirects)

### Step 10: Check DNS Configuration
```bash
nslookup api.insuranceloom.com
# or
dig api.insuranceloom.com
```

**Expected:** Should resolve to your EC2 instance's public IP

## Quick Fix Commands (Run All)

If you just want to restart everything:

```bash
# Restart API
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service --no-pager

# Restart Nginx
sudo systemctl restart nginx
sudo systemctl status nginx --no-pager

# Check both are running
sudo systemctl is-active insuranceloom-api.service
sudo systemctl is-active nginx
```

## Common Issues and Solutions

### Issue 1: API Service Not Starting
**Symptoms:** `systemctl status` shows `failed` or `inactive`

**Solution:**
```bash
# Check logs
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager

# Common causes:
# - Database connection failed (check appsettings.json)
# - Port 5000 already in use
# - Missing dependencies
# - Wrong working directory
```

### Issue 2: API Running but Not Accessible
**Symptoms:** Localhost works, but public URL doesn't

**Solution:**
- Check Nginx is running
- Check Nginx configuration
- Check security groups
- Check DNS

### Issue 3: Database Connection Error
**Symptoms:** Logs show database connection errors

**Solution:**
```bash
# Check database is running
sudo systemctl status postgresql

# Test database connection
psql -h localhost -U your_username -d your_database

# Check appsettings.json has correct connection string
sudo cat /var/www/api/appsettings.json | grep -i connectionstring
```

### Issue 4: SSL Certificate Issues
**Symptoms:** HTTPS not working, certificate errors

**Solution:**
```bash
# Check certificate
sudo certbot certificates

# Renew if needed
sudo certbot renew

# Restart nginx
sudo systemctl restart nginx
```

## Verify Everything is Working

Run this complete check:

```bash
echo "=== API Service Status ==="
sudo systemctl status insuranceloom-api.service --no-pager | head -5

echo -e "\n=== Nginx Status ==="
sudo systemctl status nginx --no-pager | head -5

echo -e "\n=== Port 5000 Listening ==="
sudo ss -tulpn | grep :5000

echo -e "\n=== Local API Test ==="
curl -s http://localhost:5000/api/servicetypes | head -20

echo -e "\n=== Public API Test ==="
curl -s https://api.insuranceloom.com/api/servicetypes | head -20
```

## Still Not Working?

1. **Check recent API logs:**
   ```bash
   sudo journalctl -u insuranceloom-api.service -f
   ```

2. **Check Nginx error logs:**
   ```bash
   sudo tail -f /var/log/nginx/error.log
   ```

3. **Check Nginx access logs:**
   ```bash
   sudo tail -f /var/log/nginx/access.log
   ```

4. **Verify API is deployed correctly:**
   ```bash
   ls -la /var/www/api/
   ```

5. **Check API configuration:**
   ```bash
   sudo cat /var/www/api/appsettings.json
   ```

