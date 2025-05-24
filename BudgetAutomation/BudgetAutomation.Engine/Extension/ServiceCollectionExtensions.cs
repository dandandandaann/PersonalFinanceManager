using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using BudgetAutomation.Engine.ApiClient;
using BudgetAutomation.Engine.Handler;
using BudgetAutomation.Engine.Handler.Command;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Service;
using Microsoft.Extensions.Options;
using SharedLibrary.LocalTesting;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;

namespace BudgetAutomation.Engine.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectSpecificServices(
        this IServiceCollection services, IConfiguration config, bool localDevelopment = false)
    {
        // Configure AWS Services
        services.AddAWSService<IAmazonSQS>();
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
        services.AddScoped<IDynamoDBContext>(sp =>
        {
            var client = sp.GetRequiredService<IAmazonDynamoDB>();
            var contextBuilder = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => client)
                .ConfigureContext(dynamoDbContextConfig => // Use the ConfigureContext method
                {
                    dynamoDbContextConfig.TableNamePrefix = LocalDevelopment.Prefix(localDevelopment);
                });
            // contextBuilder = contextBuilder.WithTableNamePrefix("DEV_");
            return contextBuilder.Build();
        });

        // Bind configurations
        var settingsSection = config.GetSection(TelegramBotSettings.Configuration);
        var settings = settingsSection.Get<TelegramBotSettings>() ??
                       throw new ArgumentNullException(nameof(TelegramBotSettings.Configuration));

        services.Configure<TelegramBotSettings>(settingsSection);
        services.AddSingleton<IValidateOptions<TelegramBotSettings>, TelegramBotSettingsValidator>();

        services.Configure<ExpenseLoggerApiClientSettings>(config.GetSection(ExpenseLoggerApiClientSettings.Configuration));
        services.AddSingleton<IValidateOptions<ExpenseLoggerApiClientSettings>, ExpenseLoggerApiClientSettingsValidator>();

        services.Configure<UserApiClientSettings>(config.GetSection(UserApiClientSettings.Configuration));
        services.AddSingleton<IValidateOptions<UserApiClientSettings>, UserApiClientSettingsValidator>();


        // Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
        services.AddHttpClient(settings.Handle)
            .AddTypedClient<ITelegramBotClient>(httpClient =>
            {
                TelegramBotClientOptions clientOptions = new(settings.Token);
                return new TelegramBotClient(clientOptions, httpClient);
            });

        // Register Api clients
        services.AddHttpClient<IExpenseLoggerApiClient, ExpenseLoggerApiClient>();
        services.AddHttpClient<IUserApiClient, UserApiClient>();

        // Register services
        services.AddTransient<SqsUpdateProcessor>();
        services.AddScoped<IChatStateService, ChatStateService>();
        services.AddScoped<IUserManagerService, UserManagerService>();
        services.AddSingleton<ISenderGateway, SenderGateway>();

        // Register handlers
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<IMessageHandler, MessageHandler>();
        services.AddScoped<ITextMessageHandler, TextMessageHandler>();
        services.AddScoped<ICommandHandler, CommandHandler>();

        // Register commands
        services.AddScoped<ICommand, LogCommand>();
        services.AddScoped<ICommand, CancelCommand>();
        services.AddScoped<ICommand, SignupCommand>();
        services.AddScoped<ICommand, SpreadsheetCommand>();

        return services;
    }
}