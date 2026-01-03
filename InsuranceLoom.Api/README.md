# Insurance Loom API

Insurance Operating System API built with ASP.NET Core 8.0 and PostgreSQL.

## Features

- **Multi-User Authentication**: Broker, Policy Holder, and Manager login systems
- **Document Management**: Upload, verify, and manage required documents (ID, Proof of Address, Medical Certificates, etc.)
- **Manager Approval Workflow**: Complete policy approval system with document verification
- **Policy Management**: Create, submit, and track insurance policies
- **Service Types**: Support for Funeral Cover, Life Insurance, Health Insurance, etc.
- **AWS S3 Integration**: Secure document storage
- **Email Notifications**: Automated email notifications for approvals, rejections, etc.

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL 15+
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Storage**: AWS S3
- **API Documentation**: Swagger/OpenAPI

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 15+ (local or AWS RDS)
- AWS Account (for S3 storage)
- SMTP Server (for email notifications)

## Setup Instructions

### 1. Database Setup

#### Option A: Local PostgreSQL
```bash
# Create database
createdb insuranceloom_dev

# Run migration script
psql -d insuranceloom_dev -f Data/Migrations/001_InitialSchema.sql
```

#### Option B: AWS RDS PostgreSQL (Free Tier)
📖 **See detailed guide**: `AWS_RDS_SETUP.md`  
🚀 **Quick start**: `AWS_RDS_QUICK_START.md`

Quick steps:
1. Create RDS PostgreSQL instance using **Free tier** template
2. Configure security group to allow your IP on port 5432
3. Get endpoint, username, and password
4. Update connection string in `appsettings.json`
5. Run migration script (see `Data/Migrations/README.md`)

### 2. Configure Application

Update `appsettings.json` with your configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-db-host;Port=5432;Database=insuranceloom;Username=your-username;Password=your-password;"
  },
  "JwtSettings": {
    "SecretKey": "your-very-long-secret-key-minimum-32-characters",
    "Issuer": "InsuranceLoom",
    "Audience": "InsuranceLoomUsers"
  },
  "AWS": {
    "Region": "af-south-1",
    "S3Bucket": "insurance-loom-documents",
    "AccessKey": "your-aws-access-key",
    "SecretKey": "your-aws-secret-key"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@insuranceloom.com",
    "Username": "your-email",
    "Password": "your-password"
  }
}
```

### 3. AWS S3 Setup

1. Create S3 bucket named `insurance-loom-documents`
2. Enable versioning
3. Set bucket policy for private access
4. Create IAM user with S3 permissions
5. Add access key and secret key to `appsettings.json`

### 4. Run the Application

```bash
# Navigate to project directory
cd InsuranceLoom.Api

# Restore packages
dotnet restore

# Run migrations (if using EF Core migrations)
dotnet ef database update

# Run the application
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

## API Endpoints

### Authentication

- `POST /api/auth/broker/login` - Broker login (Agent Number + Password)
- `POST /api/auth/policyholder/login` - Policy Holder login (Policy Number + Password)
- `POST /api/auth/manager/login` - Manager login (Email + Password)

### Document Management

- `POST /api/document/upload` - Upload document
- `GET /api/document/required?serviceType={type}&policyHolderId={id}` - Get required documents
- `GET /api/document?policyHolderId={id}&policyId={id}` - Get all documents
- `GET /api/document/{id}/download` - Get download URL
- `PUT /api/document/{id}/verify` - Verify document (Manager only)
- `DELETE /api/document/{id}` - Delete document

### Policy Approval

- `POST /api/approval/submit` - Submit policy for approval (Broker)
- `GET /api/approval/pending` - Get pending approvals (Manager)
- `GET /api/approval/{id}` - Get approval details
- `POST /api/approval/approve` - Approve policy (Manager)
- `POST /api/approval/reject` - Reject policy (Manager)
- `POST /api/approval/request-changes` - Request changes (Manager)
- `GET /api/approval/statistics` - Get approval statistics

### Manager Management

- `POST /api/manager` - Create manager (Admin only)
- `GET /api/manager` - Get all managers (Admin only)
- `GET /api/manager/{id}` - Get manager details (Admin only)
- `PUT /api/manager/{id}` - Update manager (Admin only)
- `DELETE /api/manager/{id}` - Deactivate manager (Admin only)

### Broker Operations

- `POST /api/broker/policies/{id}/submit` - Submit policy for approval
- `GET /api/broker/policies/{id}/approval-status` - Check approval status

## Authentication

All endpoints (except login) require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer {your-jwt-token}
```

## User Roles

- **Broker**: Can create policies, upload documents, submit for approval
- **PolicyHolder**: Can view policies, upload documents, submit claims
- **Manager**: Can approve/reject policies, verify documents
- **Admin**: Can manage managers and system configuration

## Database Schema

See `Data/Migrations/001_InitialSchema.sql` for complete database schema.

## Development

### Running Migrations

If using EF Core migrations:

```bash
# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Testing

Use Swagger UI at `/swagger` to test all endpoints interactively.

## Deployment

### AWS Deployment Options

1. **AWS Elastic Beanstalk**: Easy deployment for .NET applications
2. **AWS ECS/Fargate**: Container-based deployment
3. **AWS EC2**: Traditional VM deployment

### Environment Variables

For production, use environment variables instead of `appsettings.json`:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey`
- `AWS__AccessKey`
- `AWS__SecretKey`
- `EmailSettings__Username`
- `EmailSettings__Password`

## Security Considerations

1. **Change JWT Secret Key** in production
2. **Use HTTPS** for all API calls
3. **Enable CORS** only for trusted domains
4. **Encrypt sensitive data** at rest
5. **Use strong passwords** for database and AWS credentials
6. **Enable AWS S3 encryption**
7. **Implement rate limiting** for production
8. **Regular security audits**

## Support

For issues or questions, contact: info@streamyo.net

## License

© 2024 Insurance Loom by Coding Everest. All rights reserved.

