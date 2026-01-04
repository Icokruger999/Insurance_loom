# Fix 401 Unauthorized Error

## Problem
The dashboard shows 401 Unauthorized errors, meaning the authentication token is invalid or expired.

## Solutions

### Solution 1: Log Out and Log Back In
The JWT token expires after 30 minutes. Simply log out and log back in:

1. Click "Logout" in the Manager Portal
2. Log in again with your manager credentials
3. The dashboard should now work

### Solution 2: Check Token Storage
Open browser console (F12) and check:

```javascript
// Check if token exists
localStorage.getItem('managerToken')

// Check manager info
localStorage.getItem('managerInfo')
```

If either is null, you need to log in again.

### Solution 3: Verify JWT Secret Key Match
The JWT secret key must be the same on EC2 as it was when you logged in.

**On EC2, check appsettings.json:**
```bash
cat /var/www/api/appsettings.json | grep -A 5 JwtSettings
```

**The secret key must match** what was used when the token was generated. If it changed, all existing tokens become invalid.

### Solution 4: Clear Browser Storage and Re-login
1. Open browser console (F12)
2. Run:
```javascript
localStorage.clear()
sessionStorage.clear()
```
3. Refresh the page
4. Log in again

### Solution 5: Check API is Running
Verify the API service is running on EC2:

```bash
sudo systemctl status insuranceloom-api.service
```

If it's not running, start it:
```bash
sudo systemctl start insuranceloom-api.service
```

### Solution 6: Test Token Manually
You can test if your token is valid by making a direct API call:

```javascript
// In browser console
const token = localStorage.getItem('managerToken');
fetch('https://api.insuranceloom.com/api/policy-approval/agents/activity/stats', {
    headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(console.log)
.catch(console.error);
```

If you get a 401, the token is invalid/expired - log in again.

## Most Common Fix
**Just log out and log back in.** The token expires after 30 minutes of inactivity.

## After Fixing
Once you log in again, the dashboard will:
- ✅ Authenticate successfully
- ✅ Fetch data from the database
- ✅ Display real statistics from your PostgreSQL database

