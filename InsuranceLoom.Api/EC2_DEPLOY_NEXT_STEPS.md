# Next Steps After Installing Libraries on EC2

Now that you've installed .NET, Nginx, Git, and Certbot, let's deploy and configure your API.

## Step 1: Deploy Your API Code

### Option A: Clone from Git Repository (Recommended)

```bash
# Create directory
sudo mkdir -p /var/www/api
sudo chown -R ec2-user:ec2-user /var/www/api

# Clone your repository (replace with your actual GitHub URL)
cd /var/www
git clone https://github.com/YOUR_USERNAME/Insurance_loom.git

# Navigate to API project
cd Insurance_loom/InsuranceLoom.Api

# Build and publish
dotnet publish -c Release -o /var/www/api

# Set ownership
sudo chown -R ec2-user:ec2-user /var/www/api
```

### Option B: Upload Files from Your Local Machine

**On your local machine (PowerShell):**

```powershell
# Navigate to your API project
cd InsuranceLoom.Api

# Build and publish
dotnet publish -c Release -o ./publish

# Upload to EC2 (replace with your actual values)
scp -i "your-key-name.pem" -r ./publish/* ec2-user@YOUR_EC2_IP:/home/ec2-user/api/
```

**Then on EC2:**

```bash
# Move files to proper location
sudo mkdir -p /var/www/api
sudo cp -r /home/ec2-user/api/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
```

---

## Step 2: Configure appsettings.json

Edit the configuration file:

```bash
sudo nano /var/www/api/appsettings.json
```

**Update these important settings:**

1. **Connection String** - Use your RDS endpoint:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_RDS_ENDPOINT.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=YOUR_PASSWORD;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
}
```

2. **JWT Secret Key** - Change to a strong random key:
```json
"JwtSettings": {
  "SecretKey": "your-production-secret-key-min-32-characters-long-change-this",
  "Issuer": "InsuranceLoom",
  "Audience": "InsuranceLoomUsers",
  "ExpirationMinutes": 30,
  "RefreshExpirationDays": 7
}
```

3. **AWS Settings** - Already configured, but verify:
```json
"AWS": {
  "Region": "af-south-1",
  "S3Bucket": "insurance-loom-documents",
  "AccessKey": "YOUR_AWS_ACCESS_KEY",
  "SecretKey": "YOUR_AWS_SECRET_KEY"
}
```

**Save:** `Ctrl+X`, then `Y`, then `Enter`

---

## Step 3: Configure Nginx

Create Nginx configuration:

```bash
sudo nano /etc/nginx/conf.d/api.conf
```

**Paste this configuration** (replace `YOUR_EC2_IP` with your actual IP):

```nginx
server {
    listen 80;
    server_name api.insuranceloom.com YOUR_EC2_IP;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Test and reload:**

```bash
# Test configuration
sudo nginx -t

# If test passes, reload
sudo systemctl reload nginx
```

---

## Step 4: Create Systemd Service

Create service file to keep API running:

```bash
sudo nano /etc/systemd/system/insuranceloom-api.service
```

**Paste this content:**

```ini
[Unit]
Description=Insurance Loom API
After=network.target

[Service]
Type=notify
User=ec2-user
WorkingDirectory=/var/www/api
ExecStart=/usr/bin/dotnet /var/www/api/InsuranceLoom.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=insuranceloom-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

**Note:** If using Ubuntu, change `User=ec2-user` to `User=ubuntu`

**Enable and start the service:**

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (starts on boot)
sudo systemctl enable insuranceloom-api

# Start service
sudo systemctl start insuranceloom-api

# Check status
sudo systemctl status insuranceloom-api
```

**You should see:** `Active: active (running)`

---

## Step 5: Test Your API

### Test 1: Check if API is running locally

```bash
curl http://localhost:5000/api/auth/broker/login
```

Should return an error (expected - needs POST with data), but confirms API is running.

### Test 2: Check through Nginx

```bash
curl http://localhost/api/auth/broker/login
```

### Test 3: Test from browser

Open in browser:
```
http://YOUR_EC2_IP/api/auth/broker/login
```

### Test 4: Check API logs

```bash
sudo journalctl -u insuranceloom-api -f
```

Press `Ctrl+C` to exit log view.

---

## Step 6: Configure DNS (for SSL)

If you want to use `api.insuranceloom.com`:

1. **Go to Namecheap DNS settings**
2. **Add A Record:**
   - Host: `api`
   - Type: `A Record`
   - Value: `YOUR_EC2_IP`
   - TTL: Automatic

3. **Wait 5-15 minutes** for DNS propagation

4. **Then set up SSL:**
```bash
sudo certbot --nginx -d api.insuranceloom.com
```

Follow prompts. Certbot will automatically configure HTTPS.

---

## Step 7: Update Security Group

After everything is working:

1. **Go to AWS Console → EC2 → Security Groups**
2. **Find your security group**
3. **Remove port 5000** (no longer needed - Nginx handles it)
4. **Restrict SSH (port 22)** to your IP only (for security)

---

## Quick Commands Reference

### Check API Status:
```bash
sudo systemctl status insuranceloom-api
```

### Restart API:
```bash
sudo systemctl restart insuranceloom-api
```

### View API Logs:
```bash
sudo journalctl -u insuranceloom-api -f
```

### View Recent Logs:
```bash
sudo journalctl -u insuranceloom-api -n 50
```

### Check Nginx Status:
```bash
sudo systemctl status nginx
```

### Restart Nginx:
```bash
sudo systemctl restart nginx
```

### Test Nginx Config:
```bash
sudo nginx -t
```

---

## Troubleshooting

### API won't start:
```bash
# Check detailed logs
sudo journalctl -u insuranceloom-api -n 100

# Check if port is in use
sudo netstat -tlnp | grep 5000

# Verify appsettings.json is correct
cat /var/www/api/appsettings.json
```

### Connection refused errors:
- Check security group allows port 80/443
- Check API is running: `sudo systemctl status insuranceloom-api`
- Check Nginx is running: `sudo systemctl status nginx`

### Database connection errors:
- Verify RDS endpoint in appsettings.json
- Check security group allows EC2 to access RDS (port 5432)
- Verify database credentials

---

## What's Next?

Once everything is working:

1. ✅ Test broker registration: `POST http://YOUR_EC2_IP/api/auth/broker/register`
2. ✅ Test broker login: `POST http://YOUR_EC2_IP/api/auth/broker/login`
3. ✅ Update frontend API URL (already configured to use `api.insuranceloom.com`)
4. ✅ Set up SSL certificate for HTTPS
5. ✅ Monitor logs and performance

Your API should now be accessible! 🎉

