# Verify Broker Approval Status

## Check if Broker is Fully Approved

Run this query to see the exact status:

```sql
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
    b.created_at as broker_created_at,
    b.updated_at as broker_updated_at
FROM users u
LEFT JOIN brokers b ON b.user_id = u.id
WHERE u.email = 'staceykruger246@outlook.com';
```

## If Broker is Approved but Still Can't Login:

### Check 1: Both Must Be Active
Both `user_is_active` AND `broker_is_active` must be `true` for login to work.

### Check 2: Fix if User is Not Active
If `broker_is_active = true` but `user_is_active = false`, run:

```sql
UPDATE users 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE email = 'staceykruger246@outlook.com';
```

### Check 3: Verify Approval Was Saved
Check the approval history:

```sql
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

### Check 4: Complete Approval Fix
If broker is approved but user is not, run this to fix both:

```sql
-- Activate user
UPDATE users 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE email = 'staceykruger246@outlook.com';

-- Ensure broker is active
UPDATE brokers 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE user_id = (SELECT id FROM users WHERE email = 'staceykruger246@outlook.com');

-- Verify both are now active
SELECT 
    u.email,
    u.is_active as user_is_active,
    b.is_active as broker_is_active
FROM users u
JOIN brokers b ON b.user_id = u.id
WHERE u.email = 'staceykruger246@outlook.com';
```

## After Fixing, Try Login Again

The broker should now be able to log in with:
- Email: `staceykruger246@outlook.com`
- Password: (the password they set during registration)

