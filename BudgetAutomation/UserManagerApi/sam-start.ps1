Write-Host "Checking for process running on 5011..."
$proc5011 = (Get-NetTCPConnection -LocalPort 5011 -ErrorAction SilentlyContinue).OwningProcess;
if ($proc5011) {
    Stop-Process -Id $proc5011 -Force
    Write-Host "Killed process with ID $procId."
} else {
    Write-Host "No process found."
}

if ((Split-Path -Leaf (Get-Location)) -ne "UserManagerApi") {
    Set-Location UserManagerApi
}

Write-Host "Starting to build serverless template..."
sam build -t serverless.template

Write-Host "Starting SAM Local API on port 5011..."
sam local start-api -p 5011