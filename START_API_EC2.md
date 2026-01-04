# Start API on EC2 - Quick Reference

## Commands to Start the API

### 1. Check Current Status
```bash
sudo systemctl status insuranceloom-api.service
```

### 2. Start the API Service
```bash
sudo systemctl start insuranceloom-api.service
```

### 3. Enable Auto-Start on Boot (Optional)
```bash
sudo systemctl enable insuranceloom-api.service
```

### 4. Verify It's Running
```bash
sudo systemctl status insuranceloom-api.service
```

### 5. Check API Logs (if needed)
```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

### 6. Test API Endpoint
```bash
# Test from EC2 itself
curl http://localhost:5000/api/servicetypes

# Test via public URL
curl https://api.insuranceloom.com/api/servicetypes
```

## If the API Won't Start

### Check for Errors
```bash
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
```

### Restart the Service
```bash
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

### Check if Port 5000 is in Use
```bash
sudo netstat -tulpn | grep :5000
# or
sudo ss -tulpn | grep :5000
```

### Check Nginx Status (if using reverse proxy)
```bash
sudo systemctl status nginx
```

## Quick Start Command (All-in-One)
```bash
sudo systemctl start insuranceloom-api.service && sudo systemctl status insuranceloom-api.service --no-pager
```

