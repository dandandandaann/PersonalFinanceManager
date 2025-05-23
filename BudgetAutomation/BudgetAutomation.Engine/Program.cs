using BudgetAutomation.Engine.Extension;
using BudgetAutomation.Engine.LocalTesting;
using BudgetAutomation.Engine.Misc;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

var configBuilder = new ConfigurationBuilder();

var localDevelopment = builder.Environment.IsDevelopment();

var config = configBuilder.AddProjectSpecificConfigurations(localDevelopment).Build();

builder.Services.AddProjectSpecificServices(config);

// Bind test configurations
services.Configure<TelegramListenerSettings>(config.GetSection(TelegramListenerSettings.Configuration));
services.AddSingleton<IValidateOptions<TelegramListenerSettings>, TelegramListenerSettingsValidator>();
services.AddHostedService<SqsListenerForTestingService>();

var app = builder.Build();

app.Run();