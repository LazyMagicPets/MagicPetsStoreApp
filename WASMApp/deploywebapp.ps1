# Publish the web app to the S3 bucket holding the webapp
# This script relies on the existence of a systemconfig.yaml file in the folder above the 
# solution folder. This should contain at least the following:
# SystemGuid: "yourguid-here-496a-bd90-27f541ff523b"
# This guid is the guid of the system that the web app is for. If you are only doing front end 
# work, this guid should be provided to you by the back end team. If you are doing back end work,
# then review the Service project AWSTemplates folder for more information.

param([string]$AppName="storeapp",[string]$Guid)

Import-Module powershell-yaml

# Load configuration from YAML file
$filePath = "..\..\serviceconfig.yaml"
if(-not (Test-Path $filePath))
{
	Write-Host "Please create a serviceconfig.yaml file above the solution folder."
	Write-Host "Copy the serviceconfig.yaml.template file and update the values in the new file."
	exit
}

$config = Get-Content -Path $filePath | ConvertFrom-Yaml
$SystemGuid = $config.SystemGuid
if(-not $Guid.HasValue) {
	$Guid = $SystemGuid
}
$SystemName = $config.SystemName
$awsProfile = $config.Profile
$bucketName = "$SystemName---webapp-$AppName-$SystemGuid"


dotnet publish -p:Publishprofile=FolderProfile

# aws s3 cp .\bin\Release\net8.0\publish\wwwroot s3://app-store-$SystemGuid/wwwroot --recursive --profile $awsProfile

# Parameters
$localFolderPath = ".\bin\Release\net8.0\publish\wwwroot"
$s3KeyPrefix = "wwwroot"

# Ensure local folder path is absolute
$localFolderPath = Resolve-Path $localFolderPath

# Ensure $SystemGuid is set
if (-not $SystemGuid) {
    Write-Host "Error: SystemGuid is not set. Please set this variable before running the script." -ForegroundColor Red
    exit 1
}

# Test AWS CLI and profile
try {
    $awsTestCommand = "aws s3 ls --profile `"$awsProfile`""
    $awsTestResult = Invoke-Expression $awsTestCommand 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "AWS CLI test failed. Error: $awsTestResult"
    }
    Write-Host "AWS CLI test successful." -ForegroundColor Green
}
catch {
    Write-Host "Error testing AWS CLI: $_" -ForegroundColor Red
    Write-Host "Please ensure AWS CLI is installed and the profile '$awsProfile' is correctly configured." -ForegroundColor Yellow
    exit 1
}

# Perform the sync operation
$syncCommand = "aws s3 sync `"$localFolderPath`" `"s3://$bucketName/$s3KeyPrefix`" --delete --profile `"$awsProfile`""
Write-Host "Running sync command: $syncCommand"

try {
    $syncResult = Invoke-Expression $syncCommand 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Sync operation failed with exit code $LASTEXITCODE. Error: $syncResult"
    }
    Write-Host "Sync completed successfully" -ForegroundColor Green
    Write-Host $syncResult
}
catch {
    Write-Host "Error during sync operation: $_" -ForegroundColor Red
    Write-Host "Full Error Details: $($_.Exception.Message)" -ForegroundColor Red
}