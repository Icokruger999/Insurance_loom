# Update Email Password on EC2

The email password needs to be updated to `Stacey@1122` in `/var/www/api/appsettings.json` on EC2.

## Steps on EC2:

1. **SSH into EC2:**
   ```bash
   ssh -i your-key.pem ec2-user@34.246.222.13
   ```

2. **Edit appsettings.json:**
   ```bash
   sudo nano /var/www/api/appsettings.json
   ```

3. **Find the EmailSettings section and update the Password:**
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

4. **Save and exit:**
   - Press `Ctrl+O` to save
   - Press `Enter` to confirm
   - Press `Ctrl+X` to exit

5. **Restart the API service:**
   ```bash
   sudo systemctl restart insuranceloom-api.service
   ```

6. **Verify the service is running:**
   ```bash
   sudo systemctl status insuranceloom-api.service
   ```

7. **Test email (optional):**
   ```bash
   curl -X POST https://api.insuranceloom.com/api/test/email \
     -H "Content-Type: application/json" \
     -d '{"to": "ico@astutetech.co.za", "subject": "Test Email", "body": "This is a test email"}'
   ```

## Complete Email Settings:

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

**Note:** Make sure there are no extra spaces or quotes around the password value.

