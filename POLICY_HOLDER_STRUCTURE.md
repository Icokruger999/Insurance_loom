# Policy Holder Structure

## Overview
The system now distinguishes between:
- **Primary Policy Holder**: The main person who owns the policy
- **Dependents**: People covered under the policy (children, spouse, etc.)
- **Beneficiaries**: People who will receive benefits when the policy holder passes away

## Database Tables

### 1. `policy_holders` Table (Primary/Main Policy Holder)
This table stores the **primary policy holder** - the main person who owns the policy.

**Key Fields:**
- All personal information (name, ID, address, etc.)
- Employment information
- Financial information
- Agency/Employer information

**Purpose:** This is the main policy owner/account holder.

### 2. `dependents` Table (NEW)
Stores people covered under the policy but who are not the primary policy holder.

**Examples:**
- Children covered under family policy
- Spouse covered under joint policy
- Other family members

**Key Fields:**
- `policy_holder_id` - Links to the **primary policy holder**
- `policy_id` - Optional link to specific policy
- `first_name`, `last_name`, `middle_name`
- `id_number`
- `date_of_birth`
- `relationship` - Relationship to primary policy holder (Spouse, Child, Parent, etc.)
- `is_active` - Whether currently covered

**Purpose:** Track who is covered under the policy (not just who owns it).

### 3. `beneficiaries` Table
Stores people who will receive benefits when the policy holder passes away.

**Key Fields:**
- `policy_holder_id` - Links to the **primary policy holder**
- `policy_id` - Optional link to specific policy
- `full_name`, `date_of_birth`, `age`
- `mobile`, `email`
- `relationship` - Relationship to primary policy holder
- `type` - Revocable or Irrevocable
- `is_primary` - Indicates if this is the primary beneficiary (NEW)

**Purpose:** Track who receives payouts/benefits upon death of policy holder.

### 4. `policies` Table
Stores the actual insurance policy information.

**Key Fields:**
- `policy_holder_id` - Links to the **primary policy holder**
- `broker_id` - Broker who sold the policy
- `service_type_id` - Type of insurance
- Coverage amounts, premium amounts, status, etc.

**Purpose:** The actual insurance policy contract.

## Relationships

```
policy_holders (Primary/Main Policy Holder)
    ├── policies (1 to many) - Policies owned by this person
    ├── dependents (1 to many) - People covered under policies
    └── beneficiaries (1 to many) - People who receive benefits
```

## Key Concepts

### Primary Policy Holder
- The main account holder/owner of the policy
- Stored in `policy_holders` table
- Can have multiple policies
- All dependents and beneficiaries are linked to this primary holder

### Dependents
- People covered under the policy (children, spouse, etc.)
- Stored in `dependents` table
- Linked to primary policy holder via `policy_holder_id`
- Can be linked to specific policies via `policy_id`
- Have their own personal information (name, ID, DOB)
- Used for coverage purposes

### Beneficiaries
- People who receive benefits when policy holder dies
- Stored in `beneficiaries` table
- Linked to primary policy holder via `policy_holder_id`
- Can be linked to specific policies via `policy_id`
- Can mark one as `is_primary = true` for primary beneficiary
- Used for payout/benefit purposes

## Example Scenario

**Family Insurance Policy:**
- **Primary Policy Holder:** John Doe (stored in `policy_holders`)
- **Dependents:** 
  - Jane Doe (Spouse) - stored in `dependents`
  - Johnny Doe Jr. (Child) - stored in `dependents`
  - Jane Doe Jr. (Child) - stored in `dependents`
- **Beneficiaries:**
  - Jane Doe (Spouse, Primary) - stored in `beneficiaries` with `is_primary = true`
  - Johnny Doe Jr. (Child) - stored in `beneficiaries`
- **Policy:** Family Life Insurance Policy - stored in `policies`

## Database Updates

### Migration: 008_AddDependentsTableAndUpdateBeneficiaries.sql
- Creates `dependents` table
- Adds `is_primary` column to `beneficiaries` table
- Runs automatically on application startup

## API Considerations

When creating/updating policies:
1. Store primary policy holder in `policy_holders` table
2. Store dependents (children, spouse) in `dependents` table
3. Store beneficiaries in `beneficiaries` table
4. All linked via `policy_holder_id` to the primary holder
5. Optionally link to specific `policy_id` if needed

