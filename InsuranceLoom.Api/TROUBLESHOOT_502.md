# Troubleshoot 502 Bad Gateway Error

The 502 error means Nginx can't reach the API service. Check the following:

## 1. Check if API service is running:
```bash
sudo systemctl status insuranceloom-api.service
```

## 2. Check API service logs:
```bash
sudo journalctl -u insuranceloom-api.service -n 100 --no-pager
```

## 3. Check if API is listening on port 5000:
```bash
sudo netstat -tlnp | grep 5000
# OR
sudo ss -tlnp | grep 5000
```

## 4. Try starting the service:
```bash
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## 5. If service fails to start, check for errors:
```bash
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager | tail -30
```

## Common Issues:

### Issue 1: Service not running
**Solution:** Start the service
```bash
sudo systemctl start insuranceloom-api.service
sudo systemctl enable insuranceloom-api.service
```

### Issue 2: Configuration error in appsettings.json
**Solution:** Check JSON syntax is valid
```bash
sudo cat /var/www/api/appsettings.json | python3 -m json.tool
```

### Issue 3: Database connection issue
**Solution:** Check connection string is correct in appsettings.json

### Issue 4: Port already in use
**Solution:** Check what's using port 5000
```bash
sudo lsof -i :5000
```

## Quick Fix Steps:
1. Check service status
2. Check logs for errors
3. Restart service: `sudo systemctl restart insuranceloom-api.service`
4. Verify it's running: `sudo systemctl status insuranceloom-api.service`

