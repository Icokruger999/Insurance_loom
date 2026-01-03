# Check Local PostgreSQL Database

## Connect to Local PostgreSQL

**Windows (using psql):**
```bash
psql -U postgres -d insuranceloom
```

**Or if you have a different database name:**
```bash
psql -U postgres
```

Then connect to your database:
```sql
\c insuranceloom
```

## Check Broker Account Status

Run this query to check the broker:

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

## Check All Brokers

```sql
SELECT 
    u.email,
    u.is_active as user_is_active,
    b.agent_number,
    b.first_name,
    b.last_name,
    b.is_active as broker_is_active,
    b.created_at
FROM users u
LEFT JOIN brokers b ON b.user_id = u.id
WHERE u.user_type = 'Broker'
ORDER BY b.created_at DESC;
```

## Check All Users

```sql
SELECT id, email, user_type, is_active, created_at 
FROM users 
ORDER BY created_at DESC;
```

## Fix Broker Approval (if needed)

If the broker exists but isn't approved:

```sql
-- Activate user
UPDATE users 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE email = 'staceykruger246@outlook.com';

-- Activate broker
UPDATE brokers 
SET is_active = true, updated_at = CURRENT_TIMESTAMP
WHERE user_id = (SELECT id FROM users WHERE email = 'staceykruger246@outlook.com');
```

## Check Approval History

```sql
SELECT 
    bah.action,
    bah.performed_by_email,
    bah.previous_status,
    bah.new_status,
    bah.created_at,
    bah.notes,
    u.email as broker_email
FROM broker_approval_history bah
JOIN brokers b ON bah.broker_id = b.id
JOIN users u ON b.user_id = u.id
ORDER BY bah.created_at DESC
LIMIT 10;
```

