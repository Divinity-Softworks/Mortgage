# Define the service name and destination directory
$serviceName = "divinitysoftworks-workers-mortgage"
$destination = "C:\Program Files\Divinity Softworks\Mortgage Worker"

# Get the current script directory
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path

# Get the script name
$scriptName = $MyInvocation.MyCommand.Name

# Function to remove the service if it exists
function Remove-ServiceIfExists {
    param (
        [string]$serviceName
    )
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -eq 'Running') {
            Stop-Service -Name $serviceName -Force
        }
        sc.exe delete $serviceName
        Write-Output "Service $serviceName has been removed."
    } else {
        Write-Output "Service $serviceName does not exist."
    }
}

# Remove the service if it exists
Remove-ServiceIfExists -serviceName $serviceName

# Ensure the destination directory exists
if (-not (Test-Path -Path $destination)) {
    New-Item -ItemType Directory -Path $destination
}

# Get all items in the script directory except the .ps1 script
$itemsToCopy = Get-ChildItem -Path $scriptDirectory -Recurse | Where-Object { $_.Name -ne $scriptName }

# Copy each item to the destination
foreach ($item in $itemsToCopy) {
    $destinationPath = $item.FullName.Replace($scriptDirectory, $destination)
    if ($item.PSIsContainer) {
        if (-not (Test-Path -Path $destinationPath)) {
            New-Item -ItemType Directory -Path $destinationPath
        }
    } else {
        Copy-Item -Path $item.FullName -Destination $destinationPath -Force
    }
}

Write-Output "Files and folders have been copied to $destination"

# Define the path to the service executable (adjust if necessary)
$serviceExePath = Join-Path -Path $destination -ChildPath "DivinitySoftworks.Workers.Mortgage.exe"

# Install the service
New-Service -Name $serviceName -BinaryPathName "`"$serviceExePath`"" -DisplayName "Divinity Softworks - Mortgage Worker" -Description "A Windows service that periodically checks for updated mortgage rates from specified sources. This service logs all activity and supports notifications to keep you informed about the latest mortgage rates. Perfect for ensuring you always have the most current mortgage information at your fingertips." -StartupType Automatic

Write-Output "Service $serviceName has been installed."

# Start the service
Start-Service -Name $serviceName

Write-Output "Service $serviceName has been started."