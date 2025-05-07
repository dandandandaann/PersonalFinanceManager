Write-Host "Starting to build TelegramListener serverless template..."

if ((Split-Path -Leaf (Get-Location)) -ne "TelegramListener") {
    Set-Location TelegramListener
}

sam build -t serverless.template