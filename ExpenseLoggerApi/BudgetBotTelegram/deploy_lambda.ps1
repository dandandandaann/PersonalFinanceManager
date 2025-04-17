cd "C:\repo\BudgetAutomation\ExpenseLoggerApi\BudgetBotTelegram"
dotnet lambda deploy-function budget-bot-telegram --function-runtime dotnet8 --function-architecture x86_64
Read-Host -Prompt "Press Enter to exit"