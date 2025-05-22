using BudgetAutomation.Engine;
using BudgetAutomation.Engine.Misc;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

var configBuilder = new ConfigurationBuilder();

var localDevelopment = builder.Environment.IsDevelopment();
// Local development settings
var devPrefix = localDevelopment ? "dev-" : "";

// Configure AWS Parameter Store
configBuilder.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");

var config = configBuilder.Build();

builder.Services.AddBudgetAutomationCoreServices(config);

Console.WriteLine($"builder.Environment.IsDevelopment(): {builder.Environment.IsDevelopment()}");

#if DEBUG
    // Bind test configurations
    services.Configure<TelegramListenerSettings>(config.GetSection(TelegramListenerSettings.Configuration));
    services.AddSingleton<IValidateOptions<TelegramListenerSettings>, TelegramListenerSettingsValidator>();
    services.AddHostedService<SqsListenerForTestingService>();
#endif

var app = builder.Build();

app.Run();