﻿if ((Split-Path -Leaf (Get-Location)) -ne "BudgetAutomation.Engine") {
    Set-Location "BudgetAutomation.Engine"
}

Write-Host "Starting build..."
sam build -t serverless.template

Write-Host "Starting deploy..."
sam deploy --no-confirm-changeset --config-file LambdaConfig/samconfig.toml

Read-Host -Prompt "Press Enter to exit"