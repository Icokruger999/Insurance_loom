# Installing .NET SDK 8.0 on EC2

## Step 1: Check Your Linux Distribution

First, check what OS you're using:

```bash
cat /etc/os-release
```

This will tell you if you're using:
- Amazon Linux 2023
- Amazon Linux 2
- Ubuntu
- CentOS
- etc.

## Step 2: Install .NET SDK Based on Your OS

### For Amazon Linux 2023 (Recommended):

```bash
# Install .NET SDK 8.0 directly
sudo dnf install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
# Should show: 8.0.x
```

### For Amazon Linux 2:

```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/amazonlinux/2/packages-microsoft-prod.rpm

# Install .NET SDK 8.0
sudo yum install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

### For Ubuntu 22.04:

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update

# Install .NET SDK 8.0
sudo apt install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

### For Ubuntu 20.04:

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update

# Install .NET SDK 8.0
sudo apt install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

## Troubleshooting

### If you get "package not found":

1. Make sure you're using the correct repository URL for your OS version
2. Try updating your package lists first:
   - Amazon Linux 2023: `sudo dnf update`
   - Amazon Linux 2: `sudo yum update`
   - Ubuntu: `sudo apt update`

### If you already have .NET Runtime but need SDK:

The SDK includes the runtime, so you can install the SDK even if runtime is already installed.

## Next Steps After Installing SDK

Once the SDK is installed, you can build the API:

```bash
cd Insurance_loom/InsuranceLoom.Api
dotnet publish -c Release -o /var/www/api
```

