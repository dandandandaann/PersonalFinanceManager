Write-Host "Starting to build serverless template..."

if ((Split-Path -Leaf (Get-Location)) -ne "UserManagerApi") {
    Set-Location UserManagerApi
}
sam build -t serverless.template