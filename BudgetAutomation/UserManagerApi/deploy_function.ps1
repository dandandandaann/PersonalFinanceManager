if ((Split-Path -Leaf (Get-Location)) -eq "BudgetAutomation") {
    Set-Location UserManagerApi
}
else {
    Set-Location "..\UserManagerApi"
}

Write-Host "Starting build..."
sam build -t serverless.template

Write-Host "Starting deploy..."
sam deploy --no-confirm-changeset --config-file LambdaConfig/samconfig.toml

Read-Host -Prompt "Press Enter to exit"