# SSL Certificate Troubleshooting - ERR_CERT_COMMON_NAME_INVALID

## Current Issue
- **Error:** `NET::ERR_CERT_COMMON_NAME_INVALID`
- **Meaning:** The SSL certificate doesn't match the domain name yet
- **Cause:** SSL certificate is still being provisioned or domain verification incomplete

## Step 1: Verify Domain Status in AWS Amplify

1. **Go to AWS Amplify Console**
   - Navigate to: https://console.aws.amazon.com/amplify/
   - Select your app
   - Go to **App settings** → **Domain management**

2. **Check Domain Status**
   Look for your domain `insuranceloom.com` and check:
   - **Status:** Should be "Active" or "Available"
   - **SSL Certificate:** Should be "Issued" or "Active"
   
   If you see:
   - ❌ "Pending verification" → Domain ownership not verified yet
   - ❌ "SSL provisioning" → Certificate still being created
   - ✅ "Active" → Should work, might need to wait for propagation

## Step 2: Verify SSL Verification CNAME Record

The SSL verification CNAME record must be correctly configured:

1. **Go to Namecheap**
   - Domain List → Manage insuranceloom.com → Advanced DNS

2. **Verify the SSL verification CNAME exists:**
   - **Host:** `_4cf95c67727215e929649d7de29b43db`
   - **Type:** CNAME
   - **Value:** `_15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.`
   - **Must include trailing dot (.)**

3. **Check if record is correct:**
   - Make sure there are no typos
   - Ensure trailing dot is present
   - TTL should be Automatic or a low value (5-30 min)

## Step 3: Wait for SSL Certificate Provisioning

SSL certificates can take time to provision:

- **Typical time:** 15-30 minutes after DNS records are added
- **Maximum time:** Up to 2 hours in some cases
- **AWS Amplify** automatically provisions SSL certificates via AWS Certificate Manager (ACM)

## Step 4: Force Re-verification (If Needed)

If verification is stuck:

1. **In Amplify Console:**
   - Go to Domain management
   - Click on your domain
   - Look for "Re-verify" or "Retry verification" button
   - Click it to force AWS to check DNS records again

2. **Or remove and re-add domain:**
   - Remove the domain from Amplify
   - Wait 5 minutes
   - Re-add the domain
   - Follow the DNS setup process again

## Step 5: Check DNS Propagation for SSL Record

The SSL verification CNAME needs to propagate:

1. **Use online tool:**
   - Visit: https://www.whatsmydns.net/
   - Select "CNAME" record type
   - Enter: `_4cf95c67727215e929649d7de29b43db.insuranceloom.com`
   - Check if it resolves to: `_15894148a462241240cacc061954ba62.jkddzztszm.acm-validations.aws.`

2. **Command line check:**
   ```powershell
   nslookup -type=CNAME _4cf95c67727215e929649d7de29b43db.insuranceloom.com
   ```

## Step 6: Common Solutions

### Solution 1: Wait Longer
- SSL provisioning can take 30-60 minutes
- Be patient and check Amplify console periodically

### Solution 2: Verify DNS Records Are Exact
- Double-check all three DNS records in Namecheap
- Ensure no typos or missing trailing dots
- Remove any conflicting records

### Solution 3: Clear Browser Cache
- Clear browser cache and cookies
- Try incognito/private mode
- Try different browser

### Solution 4: Check Amplify Console for Errors
- Look for any error messages in Domain management
- Check if there are any warnings or issues

## Step 7: Verify in Amplify Console

**What you should see when working:**
- Domain status: **"Active"** (green)
- SSL certificate: **"Issued"** or **"Active"** (green)
- Associated URLs showing:
  - ✅ `https://insuranceloom.com` - Active
  - ✅ `https://www.insuranceloom.com` - Active

## Expected Timeline

- **0-15 minutes:** DNS propagation
- **15-30 minutes:** Domain verification
- **30-60 minutes:** SSL certificate provisioning
- **Total:** Usually 30-60 minutes, can take up to 2 hours

## If Still Not Working After 2 Hours

1. **Check Amplify Console for specific error messages**
2. **Verify all DNS records are exactly correct**
3. **Contact AWS Support** if domain shows as "Failed"
4. **Consider removing and re-adding the domain**

## Quick Checklist

- [ ] SSL verification CNAME record exists in Namecheap
- [ ] SSL verification CNAME has correct value with trailing dot
- [ ] Domain status in Amplify shows "Active" or "Available"
- [ ] SSL certificate status shows "Issued" or "Active"
- [ ] Waited at least 30-60 minutes after adding DNS records
- [ ] Checked Amplify console for any error messages

---

**Current Status:** SSL certificate provisioning in progress  
**Action:** Check Amplify console domain status and wait for SSL to complete

