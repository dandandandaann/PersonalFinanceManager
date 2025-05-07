<!-- ------------------------------------------------------------ -->

<hr id="readme-start" />

<!-- ------------------------------------------------------------ -->

[[_TOC_]]

# Personal Finance Manager

An exploration into building a personal finance toolset, starting with the challenge of quick and easy expense tracking. PersonalFinanceManager currently implements a Telegram bot that acts as a conversational interface for recording financial transactions directly into a Google Spreadsheet. The goal is to build out further budgeting and automation features over time. 
Tech Stack: .NET 8, AWS Lambda (Minimal APIs), DynamoDB, Telegram Bot API, Google Sheets API. Status: Work in Progress.

This monorepo contains source code, Infrastructure as Code (IaC), and test files intended to be compiled/deployed through MSBuild (Visual Studio) and [AWS Sam](#Citations)
<hr />

<!-- ------------------------------------------------------------ -->

## Onboarding

Initial onboarding steps are available here: still not available

## Projects

- [BudgetAutomation - SQS Worker](BudgetAutomation/BudgetBotTelegram)
  - Port: 6001
- [TelegramListener - Api](BudgetAutomation/TelegramListener)
  - API Port: 6011
- [ExpenseLoggerApi - Api](BudgetAutomation/ExpenseLoggerApi)
  - API Port: 5001
- [UserManagerApi - Api](BudgetAutomation/UserManagerApi)
  - API Port: 5011

## QA (Quality Assurance) Testing

### Categories

- SmokeTests (not implemented)
  - Rapid health check of services

### Projects

- [BudgetAutomation](BudgetAutomation/BudgetAutomation.Tests)
- [TelegramListener](BudgetAutomation/TelegramListener.Tests)
- [ExpenseLoggerApi](BudgetAutomation/ExpenseLoggerApi.Tests)
- [UserManagerApi](BudgetAutomation/UserManagerApi.Tests)

## Scripts

### `BudgetAutomation/deploy_all_functions.ps1`
  - A script you can use to call all `deploy_function.ps1` under its file structure.
  - For example, the following command will start the deploy of all lambda services
    - ```.\BudgetAutomation\deploy_all_functions.ps1```

<hr />

<!-- ------------------------------------------------------------ -->

## Citations

<div>Docker Desktop - <a href="https://www.docker.com/products/docker-desktop">Download (source)</a></div>
<div>Aws Cli - <a href="https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html">Download (source)</a></div>
<div>Aws Sam Cli - <a href="https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html">Download (source)</a></div>

<hr />

<!-- ------------------------------------------------------------ -->
