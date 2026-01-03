# Test Email Endpoint

## Send a Test Email

After deploying the API, you can test the email configuration by sending a test email.

### Using curl:

```bash
curl -X POST https://api.insuranceloom.com/api/test/email \
  -H "Content-Type: application/json" \
  -d '{
    "to": "ico@astutetech.co.za",
    "subject": "Test Email from Insurance Loom",
    "body": "This is a test email to verify email configuration is working."
  }'
```

### Simple test (uses default subject/body):

```bash
curl -X POST https://api.insuranceloom.com/api/test/email \
  -H "Content-Type: application/json" \
  -d '{"to": "ico@astutetech.co.za"}'
```

### Expected Response:

**Success (200 OK):**
```json
{
  "message": "Test email sent successfully",
  "to": "ico@astutetech.co.za"
}
```

**Error (500):**
```json
{
  "message": "Failed to send test email",
  "error": "Error details..."
}
```

## Note

This endpoint should ideally be removed or protected in production. It's useful for testing email configuration during development and deployment.

