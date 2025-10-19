<!-- ------------------------------------------------------------ -->

<hr id="readme-start" />

<!-- ------------------------------------------------------------ -->

[[_TOC_]]

# Personal Finance Manager

An exploration into building a personal finance toolset, starting with the challenge of quick and easy expense tracking. PersonalFinanceManager currently implements a Telegram bot that acts as a conversational interface for recording financial transactions directly into a Google Spreadsheet. The goal is to build out further budgeting and automation features over time. 
Tech Stack: .NET 8 (Native AOT), AWS Lambda, DynamoDB, Telegram Bot API, Google Sheets API. Status: Work in Progress.

This monorepo contains source code, Infrastructure as Code (IaC), and test files intended to be compiled/deployed through MSBuild (Visual Studio) and [AWS Sam](#citations)
<hr />

<!-- ------------------------------------------------------------ -->

## Onboarding

**Configuração Inicial:**

**Criar usuário AWS**
Seu usuário e url de acesso será fornecido para o Console AWS.

**Instalar Docker**
https://docs.docker.com/get-started/get-docker/

**Instalar AWS CLI**
https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html

**Configurar seu login na AWS CLI**
https://docs.aws.amazon.com/pt_br/cli/latest/userguide/getting-started-quickstart.html

**Criar "Credenciais de longo prazo para usuários do IAM"**
 https://docs.aws.amazon.com/pt_br/cli/latest/userguide/cli-authentication-user.html

Quando for perguntado pela região, digite us-east-2.
```SSO region [None]:us-east-2```

Quando tudo estiver pronto, teste executando ```aws ssm get-parameter --name "/dev-BudgetAutomation/ExpenseLogger/Categories/0/Name"```

**Instalar AWS SAM**
https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html

**Setup Ngrok**,
Cadastre-se em https://ngrok.com/,
Download https://dashboard.ngrok.com/get-started/setup/windows,
Configure seu cliente com o comando ```ngrok config add-authtoken YOUR_NGROK_TOKEN```
Crie um domínio ngrok grátis https://dashboard.ngrok.com/domains,
Quando tudo estiver pronto, substitua o domínio e execute ```ngrok http --url=YOUR_NGROK_DOMAIN.ngrok-free.app http://localhost:6011```

## Projects

- [BudgetAutomation.Engine - SQS Worker](BudgetAutomation/BudgetAutomation.Engine)
  - Worker service
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

- [BudgetAutomation](BudgetAutomation/BudgetAutomation.Engine.Tests)
- [TelegramListener](BudgetAutomation/TelegramListener.Tests)
- [ExpenseLoggerApi](BudgetAutomation/ExpenseLoggerApi.Tests)
- [UserManagerApi](BudgetAutomation/UserManagerApi.Tests)

## Scripts

### `BudgetAutomation/deploy_all_functions.ps1`
  - A script you can use to call all `deploy_function.ps1` under its file structure.
  - For example, the following command will start the deployment of all lambda services
    - ```.\BudgetAutomation\deploy_all_functions.ps1```

<hr />

<!-- ------------------------------------------------------------ -->

## Citations

<div>Docker Desktop - <a href="https://www.docker.com/products/docker-desktop">Download (source)</a></div>
<div>Aws Cli - <a href="https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html">Download (source)</a></div>
<div>Aws Sam Cli - <a href="https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html">Download (source)</a></div>

<hr />

<!-- ------------------------------------------------------------ -->
