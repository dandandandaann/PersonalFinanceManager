if ((Split-Path -Leaf (Get-Location)) -ne "TelegramListener") {
    Set-Location TelegramListener
}

Write-Host "Starting build..."
sam build -t LambdaConfig/serverless.template -s ./

Write-Host "Starting deploy..."
sam deploy --no-confirm-changeset

Read-Host -Prompt "Press Enter to exit"