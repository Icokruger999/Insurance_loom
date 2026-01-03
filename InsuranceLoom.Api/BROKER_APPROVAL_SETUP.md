# Broker Approval Workflow Setup

## Summary of Changes

### 1. Broker Approval Workflow
- New brokers are now created with `IsActive = false` (pending approval)
- When a broker registers, an approval request email is sent to the configured approver email
- Brokers cannot login until their account is approved (`IsActive = true`)
- Approver receives an email with "Approve" and "Reject" links

### 2. Single Login Button
- Replaced separate "Broker Login" and "Policy Holder Login" buttons with a single "Login" button
- Clicking "Login" opens a modal to select "Broker" or "Client"
- Client login is a placeholder for future implementation

### 3. Email-Based Login
- Brokers now login using their **email address** (not Agent Number)
- Agent Number is auto-generated and sent via email after registration
- Agent Number is sent again when the broker account is approved

## Required Configuration on EC2

### 1. Update appsettings.json

Edit `/var/www/api/appsettings.json` on EC2 and add/update the `BrokerApproval` section:

```json
{
  "EmailSettings": {
    "SmtpServer": "mail.privateemail.com",
    "SmtpPort": 587,
    "SenderEmail": "info@streamyo.net",
    "SenderName": "Insurance Loom",
    "Username": "info@streamyo.net",
    "Password": "Stacey@1122"
  },
  "BrokerApproval": {
    "ApproverEmail": "your-approver-email@example.com"
  }
}
```

**Important:** Replace `your-approver-email@example.com` with the actual email address that should receive broker approval requests.

### 2. Deploy Changes to EC2

```bash
cd ~/Insurance_loom
git pull origin main
cd InsuranceLoom.Api
chmod +x deploy.sh
./deploy.sh
```

### 3. Update Database Schema (if needed)

The `Broker` entity now defaults `IsActive` to `false`. Existing brokers should be fine, but new registrations will require approval.

If you need to manually approve an existing broker, you can update the database:

```sql
UPDATE brokers SET is_active = true WHERE id = 'broker-id-here';
```

## Approval Process Flow

1. **Broker Registration:**
   - Broker fills out registration form
   - Account is created with `IsActive = false`
   - Broker receives email with Agent Number and notification that account is pending approval
   - Approver receives email with broker details and approval/rejection links

2. **Approval:**
   - Approver clicks "Approve" link in email (or uses API endpoint)
   - Broker's `IsActive` is set to `true`
   - Broker receives approval confirmation email with Agent Number

3. **Rejection:**
   - Approver clicks "Reject" link in email (or uses API endpoint)
   - Broker account is deleted
   - Broker receives rejection email (if email sending succeeds)

4. **Login:**
   - Broker uses email + password to login
   - System checks if `IsActive = true`
   - If not approved, returns error: "Your account is pending approval..."

## API Endpoints

### Approval Endpoints (No authentication required - for email links)
- `POST /api/broker/{brokerId}/approve` - Approve a broker
- `POST /api/broker/{brokerId}/reject` - Reject a broker (with optional reason)

### Registration/Login Endpoints
- `POST /api/auth/broker/register` - Register new broker (requires approval)
- `POST /api/auth/broker/login` - Login with email + password (requires approval)

## Testing

1. **Test Registration:**
   ```bash
   curl -X POST https://api.insuranceloom.com/api/auth/broker/register \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "password": "Test123!",
       "firstName": "Test",
       "lastName": "Broker",
       "companyName": "Test Company"
     }'
   ```

2. **Check Approval Email:** Verify approver receives the approval request email

3. **Test Approval:** Click the approval link in the email or use:
   ```bash
   curl -X POST https://api.insuranceloom.com/api/broker/{brokerId}/approve
   ```

4. **Test Login:** After approval, broker should be able to login with email + password

## Notes

- The approval/rejection endpoints in `BrokerApprovalController.cs` don't require authentication (for email link functionality). You may want to add authentication/authorization in production.
- Email links use HTTPS URLs - make sure your API is accessible over HTTPS
- The reject endpoint currently deletes the broker account. You may want to modify this to just mark as rejected instead.

