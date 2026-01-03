# Deploy Company Feature to EC2

The companies table has been created successfully. Now deploy the code changes.

## Deployment Steps

1. **SSH into EC2:**
   ```bash
   ssh -i your-key.pem ec2-user@34.246.222.13
   ```

2. **Pull Latest Code:**
   ```bash
   cd ~/Insurance_loom
   git pull origin main
   ```

3. **Deploy API:**
   ```bash
   cd InsuranceLoom.Api
   chmod +x deploy.sh
   ./deploy.sh
   ```

4. **Verify Deployment:**
   ```bash
   sudo systemctl status insuranceloom-api.service
   ```

5. **Check Logs (if needed):**
   ```bash
   sudo journalctl -u insuranceloom-api.service -n 50 --no-pager
   ```

6. **Test Company List Endpoint:**
   ```bash
   curl https://api.insuranceloom.com/api/company
   ```
   Should return JSON with "Astutetech Data" and "Pogo Group"

7. **Test Broker Registration:**
   - Go to https://www.insuranceloom.com
   - Click Login > Broker > Register
   - Try registering with an existing company (should work)
   - Try registering with a non-existent company without checkbox (should fail)
   - Try registering with a non-existent company with checkbox checked (should create company)

## Verification

After deployment, verify:
- ✅ Companies table exists with initial data
- ✅ API endpoints respond correctly
- ✅ Broker registration validates companies
- ✅ Company creation works during registration
