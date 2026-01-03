# AWS Amplify & Namecheap Deployment Guide

## ✅ Amplify Configuration Check

Your `amplify.yml` file is correctly configured:
- ✅ Version 1 format
- ✅ Frontend build phase (no build needed for static site)
- ✅ Artifacts configured to serve all files from root directory

## Step 1: AWS Amplify Setup

### 1.1 Create Amplify App

1. **Go to AWS Amplify Console**
   - Visit: https://console.aws.amazon.com/amplify/
   - Sign in to your AWS account

2. **Create New App**
   - Click **"New app"** → **"Host web app"**
   - Select **"GitHub"** as your Git provider
   - Click **"Authorize use of GitHub"** (if not already authorized)
   - Grant AWS Amplify access to your repositories

3. **Select Repository**
   - Choose: **Icokruger999/Insurance_loom**
   - Branch: **main**
   - Click **"Next"**

4. **Build Settings**
   - Amplify should **auto-detect** your `amplify.yml` file
   - You should see:
     ```
     version: 1
     frontend:
       phases:
         build:
           commands:
             - echo "No build needed for static site"
       artifacts:
         baseDirectory: /
         files:
           - '**/*'
     ```
   - If it doesn't auto-detect, click **"Edit"** and paste the above YAML
   - Click **"Save and deploy"**

5. **Wait for Deployment**
   - Build will start automatically
   - Takes 2-3 minutes
   - You'll see build progress in real-time
   - Once complete, you'll get a temporary URL like: `https://main-xxxxx.amplifyapp.com`

### 1.2 Verify Deployment

1. Click on your app in Amplify Console
2. You should see:
   - ✅ Build status: "Succeeded" (green checkmark)
   - ✅ App URL: A working link to your site
3. Click the App URL to verify your site loads correctly

## Step 2: Add Custom Domain in Amplify

### 2.1 Configure Domain in Amplify

1. **Navigate to Domain Management**
   - In your Amplify app → **App settings** → **Domain management**
   - Click **"Add domain"**

2. **Enter Your Domain**
   - Type: `insuranceloom.com`
   - Click **"Configure domain"**

3. **Domain Configuration**
   - Amplify will automatically provision an SSL certificate (free)
   - This takes 5-10 minutes
   - Wait for status to show "Available" or "Success"

4. **Get DNS Records**
   - After SSL is provisioned, Amplify will show you DNS records
   - You'll see something like:
     ```
     Type: CNAME
     Name: www
     Value: xxxxx.cloudfront.net
     
     Type: A (or ALIAS)
     Name: @ (or blank for root)
     Value: [IP addresses or ALIAS target]
     ```
   - **IMPORTANT**: Copy these exact values - you'll need them for Namecheap

## Step 3: Configure DNS in Namecheap

### 3.1 Access Namecheap DNS Settings

1. **Log in to Namecheap**
   - Go to: https://www.namecheap.com/
   - Sign in to your account

2. **Navigate to Domain List**
   - Click **"Domain List"** from the left menu
   - Find **insuranceloom.com**
   - Click **"Manage"** next to your domain

3. **Go to Advanced DNS**
   - Click on the **"Advanced DNS"** tab
   - You'll see your current DNS records

### 3.2 Add/Update DNS Records

**IMPORTANT**: Use the exact values provided by AWS Amplify in Step 2.1

#### Option A: If Amplify provides CNAME for www and A records for root

1. **Add CNAME for www subdomain:**
   - Click **"Add New Record"**
   - Type: **CNAME Record**
   - Host: **www**
   - Value: **[The CNAME value from Amplify]** (e.g., `xxxxx.cloudfront.net`)
   - TTL: **Automatic** (or 30 min)
   - Click **Save** (checkmark icon)

2. **Add A record for root domain (@):**
   - Click **"Add New Record"**
   - Type: **A Record**
   - Host: **@** (or leave blank for root domain)
   - Value: **[The IP address from Amplify]** (usually 4 IP addresses)
   - TTL: **Automatic** (or 30 min)
   - Click **Save** (checkmark icon)
   - **Note**: If Amplify provides multiple IPs, add separate A records for each

#### Option B: If Amplify provides ALIAS record

1. **Add CNAME for www:**
   - Type: **CNAME Record**
   - Host: **www**
   - Value: **[The CNAME value from Amplify]**
   - TTL: **Automatic**
   - Click **Save**

2. **Add ALIAS for root domain:**
   - Some DNS providers support ALIAS records
   - If Namecheap doesn't support ALIAS, use the A records provided by Amplify
   - Or use CNAME flattening if available

### 3.3 Remove/Update Existing Records

1. **Check for conflicting records:**
   - Look for any existing A, CNAME, or ALIAS records for:
     - `@` (root domain)
     - `www`
   - If they exist and point elsewhere, either:
     - **Delete them** (if not needed)
     - **Update them** with Amplify values

2. **Common records to check:**
   - A Record for `@`
   - CNAME for `www`
   - Any other subdomain records you want to keep

### 3.4 Save Changes

1. After adding/updating all records, verify they're correct
2. Changes save automatically in Namecheap
3. DNS propagation typically takes:
   - **15 minutes to 2 hours** (most common)
   - Up to **48 hours** (worst case, but rare)

## Step 4: Verify Domain Setup

### 4.1 Check DNS Propagation

1. **Wait 15-30 minutes** after updating DNS
2. **Check DNS propagation:**
   - Use: https://www.whatsmydns.net/
   - Enter: `insuranceloom.com`
   - Check if it resolves to Amplify's IPs

### 4.2 Verify in Amplify

1. Go back to Amplify Console → Domain management
2. Check domain status:
   - Should show **"Active"** or **"Available"**
   - SSL certificate should be **"Issued"**

### 4.3 Test Your Site

1. Visit: `https://insuranceloom.com`
2. Visit: `https://www.insuranceloom.com`
3. Both should:
   - Load your landing page
   - Show a valid SSL certificate (padlock icon)
   - Redirect properly (www to non-www or vice versa, depending on Amplify settings)

## Troubleshooting

### Issue: Domain not resolving
- **Solution**: Wait longer (up to 48 hours), or check DNS records are correct

### Issue: SSL certificate pending
- **Solution**: Wait 10-15 minutes, Amplify auto-provisions SSL

### Issue: Site shows Amplify default page
- **Solution**: Check that `amplify.yml` is in the root of your repository

### Issue: DNS records not saving in Namecheap
- **Solution**: Make sure you're in "Advanced DNS" tab, not "Basic DNS"

### Issue: CNAME for root domain not working
- **Solution**: Use A records instead (Amplify will provide IPs)

## Quick Reference: Namecheap DNS Record Types

- **A Record**: Points domain to IP address (for root domain)
- **CNAME Record**: Points domain to another domain name (for subdomains like www)
- **@**: Represents root domain (insuranceloom.com)
- **www**: Represents www subdomain (www.insuranceloom.com)

## Next Steps After Deployment

1. ✅ Test all pages on your site
2. ✅ Test on mobile devices
3. ✅ Verify contact form (if you add backend later)
4. ✅ Set up monitoring/alerts in Amplify
5. ✅ Consider setting up custom error pages

---

**Need Help?**
- AWS Amplify Docs: https://docs.aws.amazon.com/amplify/
- Namecheap DNS Guide: https://www.namecheap.com/support/knowledgebase/article.aspx/767/10/how-to-change-dns-for-a-domain/

