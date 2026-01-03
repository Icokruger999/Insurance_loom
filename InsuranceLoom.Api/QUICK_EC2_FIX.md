# Quick Fix: API Connection Error on EC2

If you're seeing "Connection error. Please make sure the API is running on http://34.246.222.13/api", follow these steps:

## 1. Check if API is Running
```bash
sudo systemctl status insuranceloom-api.service
```

## 2. If Service is Not Running, Start It
```bash
sudo systemctl start insuranceloom-api.service
sudo systemctl enable insuranceloom-api.service  # Enable auto-start on boot
```

## 3. If Service Failed, Check Logs
```bash
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
```

## 4. Test API Locally on EC2
```bash
curl http://localhost:5000/api/auth/broker/login
```
Should return a response (even if error, means API is running)

## 5. Check if Port 5000 is Listening
```bash
sudo netstat -tlnp | grep 5000
# OR
sudo ss -tlnp | grep 5000
```

## 6. Check Nginx Status
```bash
sudo systemctl status nginx
```

## 7. Test Nginx Configuration
```bash
sudo nginx -t
```

## 8. If Everything Looks Good but Still Not Working

### Restart Everything
```bash
sudo systemctl restart insuranceloom-api.service
sudo systemctl restart nginx
```

### Check Firewall/Security Groups
- Make sure EC2 Security Group allows inbound traffic on port 80 (HTTP)
- Security Group should allow your IP or 0.0.0.0/0 for testing

### Test Direct API Access
```bash
curl http://34.246.222.13/api/auth/broker/login
```

## Common Issues:

1. **Service crashed** - Check logs for errors
2. **Database connection failed** - Check appsettings.json connection string
3. **Port conflict** - Another service using port 5000
4. **Nginx not running** - Start nginx service
5. **Security group blocking** - Check AWS Security Group rules

## Quick Deploy Latest Changes:

```bash
cd ~/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
dotnet publish -c Release -o /var/www/api
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

