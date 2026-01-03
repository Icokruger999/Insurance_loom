# Next Steps After Adding DNS Records

## ✅ What You've Completed
- Added SSL verification CNAME record
- Added ALIAS record for root domain (@)
- Added CNAME record for www subdomain

## Step 1: Return to AWS Amplify Console

1. **Go back to your Amplify domain activation page**
   - The page where you saw the DNS records to add
   - URL should be something like: `https://console.aws.amazon.com/amplify/home?region=...`

2. **Wait for automatic verification**
   - AWS Amplify automatically checks for the DNS records
   - This usually takes **5-15 minutes** after you add the records
   - You may need to **refresh the page** if it doesn't update automatically

3. **Check the status**
   - The progress tracker should show:
     - ✅ SSL creation: Complete (green checkmark)
     - ✅ SSL configuration: Complete (green checkmark)
     - ✅ Domain activation: Should complete automatically

## Step 2: Wait for DNS Propagation

DNS changes need time to propagate across the internet:

- **Typical time:** 15 minutes to 2 hours
- **Maximum time:** Up to 48 hours (rare)
- **You can check propagation status:**
  - Visit: https://www.whatsmydns.net/
  - Enter: `insuranceloom.com`
  - Check if it resolves to AWS CloudFront IPs

## Step 3: Verify Domain Status in Amplify

1. **In Amplify Console:**
   - Go to **App settings** → **Domain management**
   - Your domain `insuranceloom.com` should show:
     - Status: **"Active"** or **"Available"**
     - SSL certificate: **"Issued"** or **"Active"**

2. **Check the associated URLs:**
   - `https://insuranceloom.com` → Should be active
   - `https://www.insuranceloom.com` → Should be active

## Step 4: Test Your Site

Once DNS has propagated (usually 15-60 minutes):

1. **Test root domain:**
   - Visit: `https://insuranceloom.com`
   - Should load your Insurance Loom landing page
   - Check for padlock icon (SSL certificate working)

2. **Test www subdomain:**
   - Visit: `https://www.insuranceloom.com`
   - Should also load your landing page
   - Should redirect or work seamlessly

3. **Test on different devices:**
   - Desktop browser
   - Mobile device
   - Different browsers (Chrome, Firefox, Safari)

## Step 5: Verify Everything Works

### ✅ Checklist:
- [ ] Domain ownership verified in Amplify (green checkmark)
- [ ] SSL certificate issued and active
- [ ] `https://insuranceloom.com` loads correctly
- [ ] `https://www.insuranceloom.com` loads correctly
- [ ] SSL padlock icon shows in browser
- [ ] Site is responsive on mobile
- [ ] All pages/sections load properly

## Troubleshooting

### If domain verification is still pending:
- **Wait 15-30 minutes** - DNS propagation takes time
- **Refresh the Amplify console page**
- **Double-check DNS records** in Namecheap match exactly
- **Verify trailing dots** in CNAME/ALIAS values

### If site doesn't load:
- **Wait longer** - DNS can take up to 48 hours
- **Clear browser cache** (Ctrl+F5 or Cmd+Shift+R)
- **Try incognito/private browsing mode**
- **Check DNS propagation** at whatsmydns.net
- **Try different network** (mobile data vs WiFi)

### If SSL certificate not issued:
- **Verify the SSL verification CNAME record** is correct
- **Wait 10-15 minutes** after adding the record
- **Check Amplify console** for any error messages

## What Happens Next Automatically

Once everything is verified:
- ✅ Your site will be live at `insuranceloom.com`
- ✅ SSL certificate will be automatically renewed by AWS
- ✅ Both www and non-www will work
- ✅ Your site will be accessible worldwide

## Monitoring Your Site

After deployment, you can:
- Monitor traffic in Amplify Console → Analytics
- Set up custom error pages
- Configure redirects (www to non-www or vice versa)
- Add environment variables if needed later

---

**Current Status:** DNS records added ✅  
**Next Action:** Return to Amplify console and wait for verification  
**Expected Time:** 5-15 minutes for verification, 15-60 minutes for full DNS propagation

