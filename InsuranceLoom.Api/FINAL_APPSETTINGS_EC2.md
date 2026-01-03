# Final appsettings.json for EC2

**The `BrokerApproval` section is NO LONGER NEEDED** - we now use the manager email from the broker registration form, not a global config value.

Here's the complete file with all correct values:

```json
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
```

**Note:** You can safely remove the `BrokerApproval` section - it's not used anymore.

