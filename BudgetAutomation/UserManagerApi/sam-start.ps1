Write-Host "Checking for process running on 5011..."
$proc5011 = (Get-NetTCPConnection -LocalPort 5011 -ErrorAction SilentlyContinue).OwningProcess;
if ($proc5011) {
    Stop-Process -Id $proc5011 -Force
    Write-Host "Killed process with ID $procId."
} else {
    Write-Host "No process found."
}

Write-Host "Starting SAM Local API on port 5011..."
Set-Location UserManagerApi
sam local start-api -p 5011