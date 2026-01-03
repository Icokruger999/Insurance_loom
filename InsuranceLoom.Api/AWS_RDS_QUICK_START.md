# AWS RDS PostgreSQL - Quick Start Guide

## 🚀 Quick Setup (5 Minutes)

### 1. Create RDS Instance

1. Go to AWS Console → RDS → Create Database
2. Select **PostgreSQL** → **Free tier** template
3. Settings:
   - **DB instance identifier**: `insuranceloom-db`
   - **Master username**: `postgres`
   - **Master password**: [Create strong password - save it!]
   - **Database name**: `insuranceloom`
4. Click **Create database**

### 2. Configure Security Group

1. Wait for database to be created (5-10 minutes)
2. Click on your database → **Connectivity & security**
3. Click on **Security group** link
4. **Edit inbound rules** → **Add rule**:
   - Type: PostgreSQL
   - Port: 5432
   - Source: **My IP** (or your IP address)
5. **Save rules**

### 3. Get Connection Details

From RDS Dashboard, note:
- **Endpoint**: `insuranceloom-db.xxxxx.af-south-1.rds.amazonaws.com`
- **Port**: `5432`
- **Database**: `insuranceloom`
- **Username**: `postgres`

### 4. Update appsettings.json

Replace in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_ENDPOINT_HERE;Port=5432;Database=insuranceloom;Username=postgres;Password=YOUR_PASSWORD_HERE;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
}
```

Example:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=insuranceloom-db.abc123xyz.af-south-1.rds.amazonaws.com;Port=5432;Database=insuranceloom;Username=postgres;Password=MySecureP@ssw0rd!;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;"
}
```

### 5. Run Migration Script

**Option A: Using pgAdmin**
1. Download pgAdmin: https://www.pgadmin.org/download/
2. Add server with your RDS endpoint
3. Connect → Right-click `insuranceloom` → Query Tool
4. Open `Data/Migrations/001_InitialSchema.sql`
5. Execute

**Option B: Using psql**
```bash
psql -h YOUR_ENDPOINT_HERE -p 5432 -U postgres -d insuranceloom -f Data/Migrations/001_InitialSchema.sql
```

### 6. Test Connection

```bash
cd InsuranceLoom.Api
dotnet run
```

If successful, Swagger UI will be available at `https://localhost:5001/swagger`

## ✅ Free Tier Checklist

- [ ] Using **db.t3.micro** instance class
- [ ] **20 GB** storage (or less)
- [ ] **7 days** backup retention
- [ ] **Single AZ** (not Multi-AZ)
- [ ] Account age **< 12 months**

## 💰 Free Tier Includes

- ✅ 750 hours/month of db.t3.micro
- ✅ 20 GB General Purpose SSD storage
- ✅ 20 GB backup storage
- ✅ 100 GB data transfer out/month

## 🔒 Security Checklist

- [ ] Strong password (12+ characters)
- [ ] Security group allows only your IP
- [ ] SSL enabled in connection string
- [ ] Regular backups enabled
- [ ] Password stored securely (not in code)

## 📝 Connection String Format

```
Host=ENDPOINT;Port=5432;Database=insuranceloom;Username=postgres;Password=PASSWORD;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Prefer;
```

## 🆘 Troubleshooting

**Can't connect?**
- Check security group allows your IP
- Verify endpoint and password
- Check RDS instance status is "Available"

**Authentication failed?**
- Verify username is `postgres`
- Check password is correct
- Ensure database name is `insuranceloom`

**Connection timeout?**
- Security group may not allow your IP
- RDS instance may still be creating
- Check VPC and subnet settings

## 📚 Full Documentation

See `AWS_RDS_SETUP.md` for detailed step-by-step instructions.

