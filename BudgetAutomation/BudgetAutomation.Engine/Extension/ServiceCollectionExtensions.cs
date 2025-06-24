using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using BudgetAutomation.Engine.ApiClient;
using BudgetAutomation.Engine.Handler;
using BudgetAutomation.Engine.Handler.Command;
using BudgetAutomation.Engine.Handler.Command.Alias;
using BudgetAutomation.Engine.Interface;
using BudgetAutomation.Engine.Mapper;
using BudgetAutomation.Engine.Service;
using Microsoft.Extensions.Options;
using SharedLibrary.LocalTesting;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectSpecificServices(
        this IServiceCollection services, IConfiguration config, bool localDevelopment = false)
    {
        services.AddLogging(builder => builder.AddLambdaLogger());

        // Configure AWS Services
        services.AddAWSService<IAmazonSQS>();
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
        services.AddScoped<IDynamoDBContext>(sp =>
        {
            var client = sp.GetRequiredService<IAmazonDynamoDB>();
            var contextBuilder = new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => client)
                .ConfigureContext(dynamoDbContextConfig =>
                {
                    dynamoDbContextConfig.TableNamePrefix = LocalDevelopment.Prefix(localDevelopment);
                });
            return contextBuilder.Build();
        });

        // Bind configurations
        var settingsSection = config.GetSection(TelegramBotSettings.Configuration);
        var settings = settingsSection.Get<TelegramBotSettings>() ??
                       throw new ArgumentNullException(nameof(TelegramBotSettings.Configuration));

        services.Configure<TelegramBotSettings>(settingsSection);
        services.AddSingleton<IValidateOptions<TelegramBotSettings>, TelegramBotSettingsValidator>();

        services.Configure<SpreadsheetManagerApiClientSettings>(config.GetSection(SpreadsheetManagerApiClientSettings.Configuration));
        services.AddSingleton<IValidateOptions<SpreadsheetManagerApiClientSettings>, SpreadsheetManagerApiClientSettingsValidator>();

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
        services.AddHttpClient<ISpreadsheetManagerApiClient, SpreadsheetManagerApiClient>();
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
        services.AddScoped<ICommand, StartCommand>();
        services.AddScoped<ICommand, LogCommand>();
        services.AddScoped<ICommand, CancelCommand>();
        services.AddScoped<ICommand, SignupCommand>();
        services.AddScoped<ICommand, SpreadsheetCommand>();
        services.AddScoped<ICommand, UndoCommand>();
        services.AddScoped<ICommand, LastItemCommand>();
        services.AddScoped<ICommand, HelpCommand>();
        services.AddScoped<ICommand, ReturnSpreadsheetCommand>();

        // Register commands alias
        services.AddScoped<CommandAliasBase, RegistrarCommandAlias>();
        services.AddScoped<CommandAliasBase, PlanilhaCommandAlias>();
        services.AddScoped<CommandAliasBase, CadastrarCommandAlias>();
        services.AddScoped<CommandAliasBase, CancelarCommandAlias>();
        services.AddScoped<CommandAliasBase, MostrarUltimoCommandAlias>();
        services.AddScoped<CommandAliasBase, AjudaCommandAlias>();
        services.AddScoped<CommandAliasBase, PlanilhaUrlCommandAlias>();

        // Register mappers
        services.AddSingleton<ReplyMarkupMapper>();

        return services;
    }
}