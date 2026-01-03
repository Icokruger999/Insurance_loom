# Namecheap DNS Configuration for insuranceloom.com

## Exact DNS Records to Add in Namecheap

Based on your AWS Amplify domain activation page, here are the **exact records** you need to add:

---

## Step 1: Domain Ownership Verification (SSL Certificate)

This record is required for AWS to verify you own the domain and issue the SSL certificate.

**Add this CNAME record:**

- **Type:** `CNAME Record`
- **Host:** `_4cf95c67727215e929649d7de29b43db`
  - ⚠️ **Important:** Enter ONLY `_4cf95c67727215e929649d7de29b43db` (without `.insuranceloom.com`)
  - Namecheap will automatically append your domain name
- **Value:** `_15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.`
  - ⚠️ **Important:** Include the trailing dot (`.`) at the end
- **TTL:** `Automatic` (or 30 min)

**What it looks like in Namecheap:**
```
Type: CNAME Record
Host: _4cf95c67727215e929649d7de29b43db
Value: _15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.
TTL: Automatic
```

---

## Step 2: Point Root Domain (@) to Amplify

This makes `insuranceloom.com` (without www) point to your Amplify site.

**Add this ANAME/ALIAS record:**

- **Type:** `ANAME Record` or `ALIAS Record`
  - ⚠️ **Note:** If Namecheap doesn't support ANAME, use **A Record** with the IP addresses that Amplify provides, OR use a CNAME if Namecheap supports CNAME for root domain
- **Host:** `@` (or leave blank for root domain)
- **Value:** `d2y5kfophuc9lk.cloudfront.net`
  - ⚠️ **Important:** Include the trailing dot (`.`) at the end: `d2y5kfophuc9lk.cloudfront.net.`
- **TTL:** `Automatic` (or 30 min)

**Alternative if ANAME not available:**
If Namecheap doesn't support ANAME/ALIAS for root domain, you may need to:
1. Use A records (Amplify should provide IP addresses)
2. Or check if Namecheap supports "CNAME Flattening" for root domain

**What it looks like in Namecheap:**
```
Type: ANAME Record (or ALIAS)
Host: @
Value: d2y5kfophuc9lk.cloudfront.net.
TTL: Automatic
```

---

## Step 3: Point WWW Subdomain to Amplify

This makes `www.insuranceloom.com` point to your Amplify site.

**Add this CNAME record:**

- **Type:** `CNAME Record`
- **Host:** `www`
- **Value:** `d2y5kfophuc9lk.cloudfront.net`
  - ⚠️ **Important:** Include the trailing dot (`.`) at the end: `d2y5kfophuc9lk.cloudfront.net.`
- **TTL:** `Automatic` (or 30 min)

**What it looks like in Namecheap:**
```
Type: CNAME Record
Host: www
Value: d2y5kfophuc9lk.cloudfront.net.
TTL: Automatic
```

---

## Step-by-Step Instructions for Namecheap

### 1. Log in to Namecheap
- Go to: https://www.namecheap.com/
- Sign in to your account

### 2. Navigate to Domain Management
- Click **"Domain List"** from the left sidebar
- Find **insuranceloom.com** in your domain list
- Click **"Manage"** button next to your domain

### 3. Go to Advanced DNS
- Click on the **"Advanced DNS"** tab
- You'll see your current DNS records

### 4. Add the Three Records

**Record 1 - SSL Verification:**
1. Click **"Add New Record"** button
2. Select **"CNAME Record"** from the Type dropdown
3. In the **Host** field, enter: `_4cf95c67727215e929649d7de29b43db`
4. In the **Value** field, enter: `_15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.`
5. TTL: Select **"Automatic"**
6. Click the **checkmark (✓)** to save

**Record 2 - Root Domain:**
1. Click **"Add New Record"** button
2. Select **"ANAME Record"** (or **"ALIAS Record"** if available)
   - If neither is available, check Amplify console for A record IP addresses
3. In the **Host** field, enter: `@` (or leave blank)
4. In the **Value** field, enter: `d2y5kfophuc9lk.cloudfront.net.`
5. TTL: Select **"Automatic"**
6. Click the **checkmark (✓)** to save

**Record 3 - WWW Subdomain:**
1. Click **"Add New Record"** button
2. Select **"CNAME Record"** from the Type dropdown
3. In the **Host** field, enter: `www`
4. In the **Value** field, enter: `d2y5kfophuc9lk.cloudfront.net.`
5. TTL: Select **"Automatic"**
6. Click the **checkmark (✓)** to save

### 5. Remove Conflicting Records
- Check for any existing A, CNAME, or ANAME records for:
  - `@` (root domain)
  - `www`
- If they exist and point to different values, either:
  - **Delete them** (click the trash icon)
  - **Update them** with the Amplify values above

### 6. Verify Your Records
After adding, you should see these three records:
1. `_4cf95c67727215e929649d7de29b43db` → CNAME → `_15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.`
2. `@` → ANAME → `d2y5kfophuc9lk.cloudfront.net.`
3. `www` → CNAME → `d2y5kfophuc9lk.cloudfront.net.`

---

## After Adding Records

### 1. Return to AWS Amplify Console
- Go back to the Amplify domain activation page
- AWS will automatically detect the DNS records
- Domain ownership verification should complete within **5-15 minutes**

### 2. Wait for DNS Propagation
- DNS changes typically take **15 minutes to 2 hours** to propagate
- Can take up to **48 hours** in rare cases
- You can check propagation at: https://www.whatsmydns.net/

### 3. Verify Your Site
Once DNS propagates and Amplify verifies ownership:
- Visit: `https://insuranceloom.com`
- Visit: `https://www.insuranceloom.com`
- Both should load your landing page with a valid SSL certificate

---

## Troubleshooting

### Issue: ANAME/ALIAS not available in Namecheap
**Solution:** 
- Check if Amplify provides A record IP addresses in the console
- Use A records instead (add separate A record for each IP)
- Or contact Namecheap support about ANAME/ALIAS support

### Issue: Domain ownership not verifying
**Solution:**
- Double-check the SSL verification CNAME record is exactly correct
- Make sure there's no typo in the hostname or value
- Wait 15-30 minutes and refresh the Amplify page

### Issue: Site not loading after DNS changes
**Solution:**
- Wait longer for DNS propagation (up to 48 hours)
- Clear your browser cache
- Try accessing from a different network/device
- Check DNS propagation status at whatsmydns.net

### Issue: SSL certificate not issued
**Solution:**
- Ensure the SSL verification CNAME record is correctly added
- Wait 10-15 minutes after adding the record
- Check Amplify console for any error messages

---

## Quick Reference: Your DNS Records Summary

| Record | Type | Host | Value |
|--------|------|------|-------|
| SSL Verification | CNAME | `_4cf95c67727215e929649d7de29b43db` | `_15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.` |
| Root Domain | ANAME | `@` | `d2y5kfophuc9lk.cloudfront.net.` |
| WWW Subdomain | CNAME | `www` | `d2y5kfophuc9lk.cloudfront.net.` |

---

**Important Notes:**
- ⚠️ Always include the trailing dot (`.`) at the end of CNAME/ANAME values
- ⚠️ Don't include `.insuranceloom.com` in the Host field - Namecheap adds it automatically
- ⚠️ Changes save automatically in Namecheap - no separate "Save" button needed
- ⚠️ Return to Amplify console after adding records to complete verification

