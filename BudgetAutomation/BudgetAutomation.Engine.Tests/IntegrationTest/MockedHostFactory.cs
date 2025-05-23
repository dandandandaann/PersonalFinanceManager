using Amazon.SQS;
using BudgetAutomation.Engine.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using SharedLibrary.Settings;
using Xunit;

namespace BudgetAutomation.Engine.Tests.IntegrationTest;

// TProgram can be your actual Program class from the worker project
public class MockedHostFactory<TProgram> : IAsyncLifetime where TProgram : class
{
    private IHost? _host;
    public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Host has not been initialized.");

    // Action to allow tests to further customize mocked SQS if needed
    public Action<Mock<IAmazonSQS>>? ConfigureMockSqs { get; set; }
    // public Action<Mock<IAmazonDynamoDB>>? ConfigureMockDynamoDB { get; set; }

    // Configuration that would normally come from Parameter Store
    private readonly Dictionary<string, string?> _testConfiguration = new()
    {
        { $"{TelegramBotSettings.Configuration}:Name", "NAME" },
        { $"{TelegramBotSettings.Configuration}:Token", "1234567890:AAAAAAAAAA-ZZZZZZZ" },
        { $"{TelegramBotSettings.Configuration}:Handle", "HANDLE" },
        { $"{TelegramBotSettings.Configuration}:WebhookToken", "TEST_TELEGRAM_BOT_WEBHOOK_TOKEN" },
        { $"{TelegramBotSettings.Configuration}:StaticId", "12345" },
        { $"{ExpenseLoggerApiClientSettings.Configuration}:Url", "http://localhost:0000/api/" },
        { $"{ExpenseLoggerApiClientSettings.Configuration}:Key", "TEST_EXPENSE_API_KEY" },
        { $"{UserApiClientSettings.Configuration}:Url", "http://localhost:1111/api/" },
        { $"{UserApiClientSettings.Configuration}:Key", "TEST_USER_API_KEY" },
        { $"{TelegramListenerSettings.Configuration}:TelegramUpdateQueue", "http://localhost:2222/000000000000/test-queue" },
        { $"{TelegramListenerSettings.Configuration}:HostAddress", "http://localhost:3333/" },
        { $"{TelegramListenerSettings.Configuration}:MaxMessages", "10" },
        { $"{TelegramListenerSettings.Configuration}:WaitTimeSeconds", "20" }
    };

    public Task InitializeAsync()
    {
        var hostBuilder = Host.CreateDefaultBuilder() // Generic Host Default Builder
            .ConfigureAppConfiguration((context, conf) =>
            {
                conf.Sources.Clear(); // Clear default sources like appsettings.json if not needed for test
                conf.AddInMemoryCollection(_testConfiguration);
                // Add other test-specific config sources if necessary
                // e.g., conf.AddJsonFile("appsettings.Test.json", optional: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                // This is where services from TProgram's Program.cs would effectively be registered
                // by the default builder convention IF TProgram's Main was called.
                // However, we want to control it more explicitly for testing.

                // 1. Call your shared service registration extension method
                //    This assumes your BudgetAutomation.Engine.ServiceCollectionExtensions is accessible
                services.AddProjectSpecificServices(hostContext.Configuration);

                // 2. Register/Override services specifically for testing
                ConfigureTestServices(services, hostContext.Configuration);

                // 3. Add any IHostedService if you intend to run them.
                //    For DI tests, you might not need to add SqsListenerForTestingService
                //    if you're only testing its resolvability and not its execution.
                //    If you DO want to test its startup, add it here:
                // services.AddHostedService<SqsListenerForTestingService>(); // (or a mocked version)

                // Remove other IHostedServices from the main app if they interfere with tests
                // Example: If TProgram.CreateHostBuilder registers other hosted services.
                // services.RemoveAll(typeof(IOtherHostedService));
            });

        // If TProgram has a static CreateHostBuilder method (common pattern), you could try to call it:
        // var hostBuilder = (IHostBuilder)typeof(TProgram)
        //    .GetMethod("CreateHostBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
        //    ?.Invoke(null, new object[] { Array.Empty<string>() })!;
        //
        // Then, further configure it:
        // hostBuilder.ConfigureAppConfiguration(...)
        // hostBuilder.ConfigureServices(...)
        // OR, more simply, use Host.CreateDefaultBuilder() and then register services from your app.
        // For the shared `AddProjectSpecificServices` method, this is cleaner.

        _host = hostBuilder.Build(); // Build the host to get the ServiceProvider
        return Task.CompletedTask;
    }


    protected virtual void ConfigureTestServices(IServiceCollection services, IConfiguration config)
    {
        // Mock IAmazonSQS
        services.RemoveAll<IAmazonSQS>();
        var mockSqs = new Mock<IAmazonSQS>();
        ConfigureMockSqs?.Invoke(mockSqs);
        services.AddSingleton<IAmazonSQS>(mockSqs.Object);

        // Mock IAmazonDynamoDB and IDynamoDBContext (if needed)
        /*
        services.RemoveAll<IAmazonDynamoDB>();
        services.RemoveAll<IDynamoDBContext>();
        var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        // ConfigureMockDynamoDB?.Invoke(mockDynamoDbClient);
        services.AddSingleton<IAmazonDynamoDB>(mockDynamoDbClient.Object);
        var mockDynamoDbContext = new Mock<IDynamoDBContext>();
        services.AddScoped<IDynamoDBContext>(sp => mockDynamoDbContext.Object);
        */

        // If testing the SqsListenerForTestingService lifecycle, you might want to register it here
        // services.Configure<TelegramListenerSettings>(config.GetSection(TelegramListenerSettings.Configuration));
        // services.AddHostedService<SqsListenerForTestingService>();
    }

    public async Task DisposeAsync()
    {
        if (_host is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _host?.Dispose();
        }
        _host = null;
    }
}