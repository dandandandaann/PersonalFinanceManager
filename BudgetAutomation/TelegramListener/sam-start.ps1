Write-Host "Checking for process running on 6011..."
$proc6011 = (Get-NetTCPConnection -LocalPort 6011 -ErrorAction SilentlyContinue).OwningProcess;
if ($proc6011) {
    Stop-Process -Id $proc6011 -Force
    Write-Host "Killed process with ID $procId."
} else {
    Write-Host "No process found."
}

if ((Split-Path -Leaf (Get-Location)) -ne "TelegramListener") {
    Set-Location TelegramListener
}

Write-Host "Starting to build TelegramListener serverless template..."
sam build -t serverless.template

Write-Host "Starting SAM Local TelegramListener API on port 6011..."
sam local start-api -p 6011