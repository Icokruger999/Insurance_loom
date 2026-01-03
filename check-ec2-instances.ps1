# This script checks for EC2 instances in your AWS account
# Region: af-south-1 (Cape Town)

$region = "af-south-1"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Checking EC2 Instances" -ForegroundColor Cyan
Write-Host "Region: $region" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if AWS CLI is installed
$awsCheck = aws --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ AWS CLI not found. Please install AWS CLI first:" -ForegroundColor Red
    Write-Host "  https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✓ AWS CLI found: $awsCheck" -ForegroundColor Green
}

# Check if credentials are configured
Write-Host ""
Write-Host "Checking AWS credentials..." -ForegroundColor Yellow
$null = aws sts get-caller-identity 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ AWS credentials are configured" -ForegroundColor Green
} else {
    Write-Host "✗ AWS credentials not configured" -ForegroundColor Red
    Write-Host "  Please configure AWS credentials using:" -ForegroundColor Yellow
    Write-Host "  aws configure" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Checking EC2 instances in region: $region" -ForegroundColor Cyan
Write-Host ""

# Get all EC2 instances
$result = aws ec2 describe-instances `
    --region $region `
    --query 'Reservations[*].Instances[*].[InstanceId,State.Name,InstanceType,PublicIpAddress,PrivateIpAddress,Tags[?Key==`Name`].Value|[0],LaunchTime]' `
    --output json 2>&1

if ($LASTEXITCODE -eq 0) {
    $instances = $result | ConvertFrom-Json
    
    # Flatten the nested array structure
    $flatInstances = @()
    foreach ($reservation in $instances) {
        foreach ($instance in $reservation) {
            if ($instance) {
                $flatInstances += $instance
            }
        }
    }

    if ($flatInstances.Count -eq 0) {
        Write-Host "ℹ No EC2 instances found in region: $region" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "This is normal if you're using:" -ForegroundColor White
        Write-Host "  - AWS Amplify (for frontend hosting)" -ForegroundColor Gray
        Write-Host "  - AWS RDS (for database)" -ForegroundColor Gray
        Write-Host "  - AWS Lambda (for serverless API)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "EC2 instances are only needed if you're deploying a traditional VM-based application." -ForegroundColor White
    } else {
        Write-Host "Found $($flatInstances.Count) EC2 instance(s):" -ForegroundColor Green
        Write-Host ""
        
        $index = 1
        foreach ($instance in $flatInstances) {
            $instanceId = $instance[0]
            $state = $instance[1]
            $instanceType = $instance[2]
            $publicIp = $instance[3]
            $privateIp = $instance[4]
            $name = if ($instance[5]) { $instance[5] } else { "N/A" }
            $launchTime = $instance[6]

            Write-Host "Instance #$index" -ForegroundColor Cyan
            Write-Host "  Instance ID: $instanceId" -ForegroundColor White
            Write-Host "  Name: $name" -ForegroundColor White
            $stateColor = if ($state -eq "running") { "Green" } else { "Yellow" }
            Write-Host "  State: $state" -ForegroundColor $stateColor
            Write-Host "  Type: $instanceType" -ForegroundColor White
            $publicIpDisplay = if ($publicIp) { $publicIp } else { 'N/A' }
            Write-Host "  Public IP: $publicIpDisplay" -ForegroundColor White
            $privateIpDisplay = if ($privateIp) { $privateIp } else { 'N/A' }
            Write-Host "  Private IP: $privateIpDisplay" -ForegroundColor White
            Write-Host "  Launch Time: $launchTime" -ForegroundColor White
            Write-Host ""
            $index++
        }

        # Summary
        $running = ($flatInstances | Where-Object { $_[1] -eq "running" }).Count
        $stopped = ($flatInstances | Where-Object { $_[1] -eq "stopped" }).Count
        $pending = ($flatInstances | Where-Object { $_[1] -eq "pending" }).Count
        $other = $flatInstances.Count - $running - $stopped - $pending

        Write-Host "Summary:" -ForegroundColor Cyan
        Write-Host "  Running: $running" -ForegroundColor Green
        Write-Host "  Stopped: $stopped" -ForegroundColor Yellow
        Write-Host "  Pending: $pending" -ForegroundColor Yellow
        if ($other -gt 0) {
            Write-Host "  Other: $other" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "✗ Error retrieving EC2 instances" -ForegroundColor Red
    Write-Host "  $result" -ForegroundColor Yellow
    Write-Host "  Make sure you have EC2 permissions in your AWS account" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "To check instances in a different region, modify the `$region variable at the top of this script." -ForegroundColor Gray
Write-Host "To check all regions, you can run:" -ForegroundColor Gray
Write-Host "  aws ec2 describe-regions --query 'Regions[*].RegionName' --output text" -ForegroundColor White
