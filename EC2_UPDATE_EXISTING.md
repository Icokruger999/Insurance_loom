# EC2 Update Commands (Existing Deployment)

Since you've deployed before, the repository is likely in a workspace directory. Use these commands:

## Quick Update Commands

```bash
# Option 1: If repository is in ~/workspace/Insurance_loom
cd ~/workspace/Insurance_loom/InsuranceLoom.Api
git pull origin main
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## If you're not sure where the repository is:

```bash
# Find the repository
find ~ -name "Insurance_loom" -type d 2>/dev/null

# Or check common locations
ls -la ~/workspace/
ls -la ~/Insurance_loom/
ls -la ~/
```

## Alternative: Clone fresh if needed

If you can't find the existing repository:

```bash
cd ~
mkdir -p workspace
cd workspace
rm -rf Insurance_loom  # Remove old if exists
git clone https://github.com/Icokruger999/Insurance_loom.git
cd Insurance_loom/InsuranceLoom.Api
dotnet publish -c Release -o ./publish
sudo systemctl stop insuranceloom-api.service
sudo cp -r ./publish/* /var/www/api/
sudo chown -R ec2-user:ec2-user /var/www/api
sudo systemctl start insuranceloom-api.service
sudo systemctl status insuranceloom-api.service
```

## Verify Deployment

```bash
# Check service status
sudo systemctl status insuranceloom-api.service

# View logs
sudo journalctl -u insuranceloom-api.service -n 50 --no-pager

# Test API endpoint
curl https://api.insuranceloom.com/api/servicetypes
```

