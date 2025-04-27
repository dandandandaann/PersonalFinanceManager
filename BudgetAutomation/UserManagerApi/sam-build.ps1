#!/bin/bash
Write-Host "Starting to build serverless template..."
Set-Location UserManagerApi
sam build -t serverless.template