#Set-Location "ExpenseLoggerApi"
dotnet lambda deploy-function expense-logger --function-runtime dotnet8 --function-architecture x86_64

Read-Host -Prompt "Press Enter to exit"