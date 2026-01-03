# Check Broker Login Issue - Database Query

## Check if Broker Exists and Status

Run this query on EC2 to check the broker account:

```sql
-- Connect to database
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom

-- Check if broker exists with this email
SELECT 
    u.id as user_id,
    u.email,
    u.user_type,
    u.is_active as user_is_active,
    b.id as broker_id,
    b.agent_number,
    b.first_name,
    b.last_name,
    b.is_active as broker_is_active,
    b.created_at as broker_created_at
FROM users u
LEFT JOIN brokers b ON b.user_id = u.id
WHERE u.email = 'staceykruger246@outlook.com';
```

## Possible Issues and Solutions:

### Issue 1: Broker Not Found
**If the query returns no rows:**
- The email is not registered
- Need to register the broker first

### Issue 2: Broker Not Approved
**If `broker_is_active = false`:**
- The broker is registered but waiting for manager approval
- Check if approval email was sent to manager
- Manager needs to click the approval link

### Issue 3: User Not Active
**If `user_is_active = false`:**
- The user account is inactive
- This can happen if the account was deactivated

### Issue 4: Wrong Password
**If broker exists and is approved:**
- The password might be incorrect
- Can reset password or check what password was set during registration

## Quick Fix - Approve Broker Manually (if needed):

If the broker exists but is not approved, you can approve them manually:

```sql
-- Approve the broker
UPDATE brokers 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE id = (SELECT b.id FROM brokers b 
            JOIN users u ON b.user_id = u.id 
            WHERE u.email = 'staceykruger246@outlook.com');

-- Also activate the user
UPDATE users 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE email = 'staceykruger246@outlook.com';
```

## Check Approval History:

```sql
-- Check if broker approval was logged
SELECT 
    bah.action,
    bah.performed_by_email,
    bah.previous_status,
    bah.new_status,
    bah.created_at,
    bah.notes
FROM broker_approval_history bah
JOIN brokers b ON bah.broker_id = b.id
JOIN users u ON b.user_id = u.id
WHERE u.email = 'staceykruger246@outlook.com'
ORDER BY bah.created_at DESC;
```

