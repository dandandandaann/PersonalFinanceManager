﻿if ((Split-Path -Leaf (Get-Location)) -eq "BudgetAutomation") {
    Set-Location SpreadsheetManagerApi
}
else {
    Set-Location "..\SpreadsheetManagerApi"
}

Write-Host "Starting build..."
sam build -t serverless.template

Write-Host "Starting deploy..."
sam deploy --config-file LambdaConfig/samconfig.toml --no-confirm-changeset

Read-Host -Prompt "Press Enter to exit"