# How to Update appsettings.json on EC2

## Step 1: Edit the file

On your EC2 instance, run:
```bash
sudo nano /var/www/api/appsettings.json
```

## Step 2: Update these sections

### Connection String (IMPORTANT)

Replace the connection string with your actual RDS endpoint:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_RDS_ENDPOINT.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=YOUR_DATABASE_PASSWORD;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
}
```

**To find your RDS endpoint:**
- Go to AWS Console → RDS → Databases
- Click on your database instance
- Copy the "Endpoint" value
- Replace `YOUR_RDS_ENDPOINT` with this value
- Replace `YOUR_DATABASE_PASSWORD` with your actual database password

### JWT Secret Key (IMPORTANT for Production)

Change the secret key to a strong random value:

```json
"JwtSettings": {
  "SecretKey": "change-this-to-a-strong-random-key-minimum-32-characters-long-for-production-use",
  "Issuer": "InsuranceLoom",
  "Audience": "InsuranceLoomUsers",
  "ExpirationMinutes": 30,
  "RefreshExpirationDays": 7
}
```

**Generate a strong key:**
```bash
# On EC2, you can generate a random key:
openssl rand -base64 32
```

### AWS Settings (Already configured, but verify)

```json
"AWS": {
  "Region": "af-south-1",
  "S3Bucket": "insurance-loom-documents",
  "AccessKey": "YOUR_AWS_ACCESS_KEY",
  "SecretKey": "YOUR_AWS_SECRET_KEY"
}
```

### Email Settings (Optional - update if needed)

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "noreply@insuranceloom.com",
  "SenderName": "Insurance Loom",
  "Username": "your-email",
  "Password": "your-password"
}
```

## Step 3: Save and Exit

In nano editor:
1. Press `Ctrl + X` to exit
2. Press `Y` to confirm save
3. Press `Enter` to confirm filename

## Step 4: Verify the file

```bash
cat /var/www/api/appsettings.json
```

## Complete Example

Here's what the complete file should look like (with your actual values):

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
    "DefaultConnection": "Host=your-rds-endpoint.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=your-actual-password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
  },
  "JwtSettings": {
    "SecretKey": "your-strong-random-secret-key-at-least-32-characters-long",
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
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@insuranceloom.com",
    "SenderName": "Insurance Loom",
    "Username": "your-email",
    "Password": "your-password"
  }
}
```

## Quick Commands

**Edit file:**
```bash
sudo nano /var/www/api/appsettings.json
```

**Generate random JWT secret:**
```bash
openssl rand -base64 32
```

**View file:**
```bash
cat /var/www/api/appsettings.json
```

**After updating, restart the API:**
```bash
sudo systemctl restart insuranceloom-api
```

