# Update Email Password on EC2

The email password needs to be updated in `/var/www/api/appsettings.json` on EC2.

## Steps:

1. **SSH into EC2:**
   ```bash
   ssh -i your-key.pem ec2-user@34.246.222.13
   ```

2. **Edit appsettings.json:**
   ```bash
   sudo nano /var/www/api/appsettings.json
   ```

3. **Update the EmailSettings section:**
   Change the Password field to: `Stacey@1122`

   The EmailSettings section should look like:
   ```json
   "EmailSettings": {
     "SmtpServer": "mail.privateemail.com",
     "SmtpPort": 587,
     "SenderEmail": "info@streamyo.net",
     "SenderName": "Insurance Loom",
     "Username": "info@streamyo.net",
     "Password": "Stacey@1122"
   }
   ```

4. **Save and exit (Ctrl+O, Enter, Ctrl+X)**

5. **Restart the API service:**
   ```bash
   sudo systemctl restart insuranceloom-api.service
   ```

6. **Test email again:**
   ```bash
   curl -X POST https://api.insuranceloom.com/api/test/email \
     -H "Content-Type: application/json" \
     -d '{"to": "ico@astutetech.co.za"}'
   ```

