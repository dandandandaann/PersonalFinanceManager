if ((Split-Path -Leaf (Get-Location)) -ne "BudgetBotTelegram") {
    Set-Location BudgetBotTelegram
}

Write-Host "Starting build..."
sam build -t serverless.template

Write-Host "Starting deploy..."
sam deploy --no-confirm-changeset

Read-Host -Prompt "Press Enter to exit"