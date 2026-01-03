# Setup HTTPS for API on EC2

## Problem
Frontend is served over HTTPS but API is HTTP, causing "Mixed Content" errors.

## Solution: Use Let's Encrypt SSL Certificate

### Step 1: Set up API Subdomain DNS
In Namecheap DNS settings, add:
- Type: A Record
- Host: `api`
- Value: `34.246.222.13`
- TTL: Automatic

Wait 5-15 minutes for DNS propagation.

### Step 2: Install Certbot on EC2
```bash
sudo dnf install -y certbot python3-certbot-nginx
# OR for Amazon Linux 2023:
sudo yum install -y certbot python3-certbot-nginx
```

### Step 3: Get SSL Certificate
```bash
sudo certbot --nginx -d api.insuranceloom.com
```

Follow the prompts:
- Enter email address
- Agree to terms
- Choose whether to redirect HTTP to HTTPS (recommend: Yes)

### Step 4: Update Nginx Configuration
Certbot will automatically update `/etc/nginx/conf.d/api.conf` to include SSL.

### Step 5: Test SSL Certificate
```bash
curl https://api.insuranceloom.com/api/auth/broker/login
```

### Step 6: Update Frontend API URL
Update `script.js` to use:
```javascript
return 'https://api.insuranceloom.com/api';
```

### Step 7: Auto-renewal (Setup)
Certbot sets up auto-renewal automatically. Test renewal:
```bash
sudo certbot renew --dry-run
```

## Quick Temporary Workaround (Not Recommended for Production)

If you need it working immediately before SSL is set up, you can temporarily update the frontend to use HTTP, but this will show security warnings.

