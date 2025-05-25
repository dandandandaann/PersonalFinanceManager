if ((Split-Path -Leaf (Get-Location)) -ne "UserManagerApi") {
    Set-Location UserManagerApi
}

Write-Host "Starting build..."
#Set-Location UserManagerApi
sam build -t LambdaConfig/serverless.template -s ./

Write-Host "Starting deploy..."
sam deploy --no-confirm-changeset --config-file LambdaConfig/samconfig.toml

Read-Host -Prompt "Press Enter to exit"