# EC2 API Troubleshooting Guide

## Connection Error: API Not Reachable

If you're seeing "Connection error. Please make sure the API is running on https://api.insuranceloom.com/api", follow these steps:

### 1. Check if API Service is Running

```bash
sudo systemctl status insuranceloom-api.service
```

### 2. Check API Logs for Errors

```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

### 3. Test API Endpoint Directly

```bash
# Test from EC2 itself
curl http://localhost:5000/api/servicetypes

# Test via public URL
curl https://api.insuranceloom.com/api/servicetypes
```

### 4. Check if Port 5000 is Listening

```bash
sudo netstat -tulpn | grep :5000
# or
sudo ss -tulpn | grep :5000
```

### 5. Restart API Service

```bash
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

### 6. Check Nginx Configuration

```bash
# Check Nginx status
sudo systemctl status nginx

# Check Nginx config for API proxy
sudo cat /etc/nginx/sites-available/api.insuranceloom.com

# Test Nginx configuration
sudo nginx -t

# Restart Nginx if needed
sudo systemctl restart nginx
```

### 7. Verify Firewall/Security Group

Make sure your EC2 security group allows:
- Inbound: Port 443 (HTTPS)
- Inbound: Port 80 (HTTP)
- Outbound: All traffic (for API to reach database)

### 8. Check SSL Certificate

```bash
# Check if SSL cert is valid
sudo certbot certificates

# Renew if needed
sudo certbot renew
```

### 9. Common Issues and Fixes

**Issue: Service fails to start**
```bash
# Check detailed error
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager

# Check if appsettings.json is correct
sudo cat /var/www/api/appsettings.json | python3 -m json.tool

# Check database connection
# (test from app logs)
```

**Issue: Port already in use**
```bash
# Find what's using port 5000
sudo lsof -i :5000
# or
sudo netstat -tulpn | grep :5000

# Kill process if needed (be careful!)
sudo kill -9 <PID>
```

**Issue: Permission errors**
```bash
# Fix permissions
sudo chown -R ec2-user:ec2-user /var/www/api
sudo chmod -R 755 /var/www/api
```

### 10. Full Deployment Retry

If nothing works, try a fresh deployment:

```bash
cd /home/ec2-user/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
sudo systemctl stop insuranceloom-api.service
dotnet clean
dotnet publish -c Release -o ./publish
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

