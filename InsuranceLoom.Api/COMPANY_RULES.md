# Company Management Rules

## Company Creation
- **Only Managers can create companies** via the `/api/company` POST endpoint
- Brokers **cannot** create companies
- Companies must be created by managers before brokers can use them

## Broker Registration
- Company name is **required** and must match an existing active company
- Brokers must select from the list of existing companies
- If a company doesn't exist, broker registration will fail with error: "Company '{name}' does not exist. Please select an existing company."
- No option to create companies during broker registration

## Company List
- Companies list is available to anyone (anonymous access) for the registration form
- Only active companies are shown in the list by default

## Workflow
1. Manager creates companies via API endpoint
2. Broker selects from existing companies during registration
3. If company doesn't exist, broker registration is rejected

