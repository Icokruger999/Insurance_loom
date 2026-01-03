# Final appsettings.json for EC2

**The `BrokerApproval` section is NO LONGER NEEDED** - we now use the manager email from the broker registration form, not a global config value.

Here's the complete file (use your actual credentials from EC2):

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
    "DefaultConnection": "Host=insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=YOUR_DB_PASSWORD;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_JWT_SECRET_KEY",
    "Issuer": "InsuranceLoom",
    "Audience": "InsuranceLoomUsers",
    "ExpirationMinutes": 30,
    "RefreshExpirationDays": 7
  },
  "AWS": {
    "Region": "af-south-1",
    "S3Bucket": "insurance-loom-documents",
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY"
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

