# Check PostgreSQL Database on EC2

## Connect to Database

```bash
# Connect to PostgreSQL
psql -h insuranceloom-db.clm264kc2ifj.af-south-1.rds.amazonaws.com -U postgres -d insuranceloom
```

Enter password when prompted: `1bHiVZ0odtB?&+S$`

## Check Users Table

```sql
-- Check all users
SELECT id, email, user_type, is_active, created_at 
FROM users 
ORDER BY created_at DESC;

-- Check for specific email
SELECT id, email, user_type, is_active, created_at 
FROM users 
WHERE email = 'ico@astutetech.co.za';
```

## Check Brokers Table

```sql
-- Check all brokers
SELECT 
    b.id,
    b.agent_number,
    b.first_name,
    b.last_name,
    b.company_name,
    b.phone,
    b.license_number,
    b.commission_rate,
    b.is_active,
    b.created_at,
    u.email as user_email,
    u.is_active as user_is_active
FROM brokers b
LEFT JOIN users u ON b.user_id = u.id
ORDER BY b.created_at DESC;

-- Check for specific email
SELECT 
    b.id,
    b.agent_number,
    b.first_name,
    b.last_name,
    b.company_name,
    b.phone,
    b.is_active,
    b.created_at,
    u.email as user_email,
    u.is_active as user_is_active
FROM brokers b
LEFT JOIN users u ON b.user_id = u.id
WHERE u.email = 'ico@astutetech.co.za';
```

## Check Managers Table

```sql
-- Check all managers
SELECT 
    m.id,
    m.email,
    m.first_name,
    m.last_name,
    m.company_id,
    c.name as company_name,
    m.is_active,
    m.created_at,
    u.email as user_email,
    u.is_active as user_is_active
FROM managers m
LEFT JOIN users u ON m.user_id = u.id
LEFT JOIN companies c ON m.company_id = c.id
ORDER BY m.created_at DESC;

-- Check for specific email
SELECT 
    m.id,
    m.email,
    m.first_name,
    m.last_name,
    m.company_id,
    c.name as company_name,
    m.is_active,
    m.created_at,
    u.email as user_email,
    u.is_active as user_is_active
FROM managers m
LEFT JOIN users u ON m.user_id = u.id
LEFT JOIN companies c ON m.company_id = c.id
WHERE m.email = 'ico@astutetech.co.za' OR u.email = 'ico@astutetech.co.za';
```

## Check Companies Table

```sql
-- Check all companies
SELECT id, name, is_active, created_at 
FROM companies 
ORDER BY created_at DESC;
```

## Check Recent Registrations

```sql
-- Check recent users created in last 24 hours
SELECT id, email, user_type, is_active, created_at 
FROM users 
WHERE created_at > NOW() - INTERVAL '24 hours'
ORDER BY created_at DESC;

-- Check recent brokers created in last 24 hours
SELECT 
    b.id,
    b.agent_number,
    b.first_name,
    b.last_name,
    b.company_name,
    b.is_active,
    b.created_at,
    u.email as user_email
FROM brokers b
LEFT JOIN users u ON b.user_id = u.id
WHERE b.created_at > NOW() - INTERVAL '24 hours'
ORDER BY b.created_at DESC;
```

## Exit PostgreSQL

```sql
\q
```

