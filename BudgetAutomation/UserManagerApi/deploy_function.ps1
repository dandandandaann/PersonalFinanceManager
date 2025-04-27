Write-Host "Starting build..."
#Set-Location UserManagerApi
sam build -t serverless.template

Write-Host "Starting deploy..."
sam deploy --no-confirm-changeset