# Fix appsettings.json on EC2 - File is Empty!

## Critical Issue
Your `appsettings.json` file is **empty**, which is causing the API to fail. Also, you're using `www-data` which doesn't exist on Amazon Linux - use `ec2-user` instead.

## Fix Steps

**1. Check the file (it's probably empty):**
```bash
cat /var/www/api/appsettings.json
```

**2. Create the file with the correct content:**
```bash
sudo tee /var/www/api/appsettings.json > /dev/null <<'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=1bHiVZ0odtB?&+S$;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
  },
  "JwtSettings": {
    "SecretKey": "InsuranceLoom-Production-JWT-Secret-Key-2024-Min-32-Chars-Long-Secure-Random-Key-For-Authentication",
    "Issuer": "InsuranceLoom",
    "Audience": "InsuranceLoomUsers",
    "ExpirationMinutes": 30,
    "RefreshExpirationDays": 7
  },
  "AWS": {
    "Region": "af-south-1",
    "S3Bucket": "insurance-loom-documents",
    "AccessKey": "AKIASFECYFH6UE4NTOMH",
    "SecretKey": "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"
  },
  "EmailSettings": {
    "SmtpServer": "mail.privateemail.com",
    "SmtpPort": 587,
    "SenderEmail": "info@streamyo.net",
    "SenderName": "Insurance Loom",
    "Username": "info@streamyo.net",
    "Password": "Stacey@1122"
  }
}
EOF
```

**3. Set permissions (use `ec2-user` not `www-data` for Amazon Linux):**
```bash
sudo chown ec2-user:ec2-user /var/www/api/appsettings.json
sudo chmod 644 /var/www/api/appsettings.json
```

**4. Validate JSON:**
```bash
cat /var/www/api/appsettings.json | python3 -m json.tool
```

**5. Restart the service:**
```bash
sudo systemctl restart insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## Alternative: Use nano if the tee command doesn't work

If the `tee` command above doesn't work, use nano:

```bash
sudo nano /var/www/api/appsettings.json
```

Then copy-paste the JSON content above, save (Ctrl+X, Y, Enter), and run steps 3-5.

