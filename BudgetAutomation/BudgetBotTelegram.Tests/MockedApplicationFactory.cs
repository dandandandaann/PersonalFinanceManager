using Microsoft.AspNetCore.TestHost;

namespace BudgetBotTelegram.Tests;

using Amazon.SQS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SharedLibrary.Settings;

public class MockedApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // Action to allow tests to further customize mocked SQS if needed
    public Action<Mock<IAmazonSQS>>? ConfigureMockSqs { get; set; }
    // public Action<Mock<IAmazonDynamoDB>>? ConfigureMockDynamoDB { get; set; } // For DynamoDB

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            // 1. Remove or prevent SystemsManager from running
            //    One way is to clear existing sources if you're sure no other
            //    essential file-based config (like appsettings.json for tests) is needed.
            //    Or, more targeted: find and remove the SystemsManager source if possible.
            //    For simplicity in tests, often clearing and adding specific test config is easier.
            conf.Sources.Clear(); // Clears all configuration sources

            // 2. Add in-memory configuration for what SystemsManager would provide
            //    These keys MUST match what AddSystemsManager expects and what your
            //    services bind to (e.g., "TelegramBotSettings:Token")
            var testConfig = new Dictionary<string, string?>
            {
                // --- TelegramBotSettings ---
                // The key path is composed of the prefix used in AddSystemsManager
                // and the section name in your Configure<TelegramBotSettings> call.
                // Since AddSystemsManager uses $"/{isLocalDev}{BudgetAutomationSettings.Configuration}/",
                // and then config.GetSection(TelegramBotSettings.Configuration) is called,
                // the effective keys for binding are just "TelegramBotSettings:PropertyName".
                // Example:
                { $"{TelegramBotSettings.Configuration}:Name", "NAME" },
                { $"{TelegramBotSettings.Configuration}:Token", "1234567890:AAAAAAAAAA-ZZZZZZZ" },
                { $"{TelegramBotSettings.Configuration}:Handle", "HANDLE" },
                { $"{TelegramBotSettings.Configuration}:WebhookToken", "TEST_TELEGRAM_BOT_WEBHOOK_TOKEN" },
                { $"{TelegramBotSettings.Configuration}:StaticId", "12345" },

                // --- ExpenseLoggerApiClientSettings ---
                { $"{ExpenseLoggerApiClientSettings.Configuration}:Url", "http://localhost:0000/api/" },
                { $"{ExpenseLoggerApiClientSettings.Configuration}:Key", "TEST_EXPENSE_API_KEY" },

                // --- UserApiClientSettings ---
                { $"{UserApiClientSettings.Configuration}:Url", "http://localhost:1111/api/" },
                { $"{UserApiClientSettings.Configuration}:Key", "TEST_USER_API_KEY" },

                // --- TelegramListenerSettings (for DEBUG) ---
                { $"{TelegramListenerSettings.Configuration}:TelegramUpdateQueue", "http://localhost:2222/000000000000/test-queue" },
                { $"{TelegramListenerSettings.Configuration}:HostAddress", "http://localhost:3333/" },
                { $"{TelegramListenerSettings.Configuration}:MaxMessages", "10" },
                { $"{TelegramListenerSettings.Configuration}:WaitTimeSeconds", "20" }

                // Add any other settings that would normally come from Parameter Store
            };
            conf.AddInMemoryCollection(testConfig);

            // Optionally, add appsettings.json or appsettings.Development.json if they contain
            // non-sensitive defaults useful for tests.
            // conf.AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
        {
            // This runs BEFORE your Program.cs's ConfigureServices

            // You could try removing existing registrations if you want to be absolutely sure,
            // but ConfigureTestServices (below) is generally preferred for overriding.
        });

        builder.ConfigureTestServices(services =>
        {
            // This runs AFTER your Program.cs's ConfigureServices,
            // allowing you to safely replace or modify registrations.

            // 3. Mock IAmazonSQS
            services.RemoveAll<IAmazonSQS>(); // Remove existing registration
            var mockSqs = new Mock<IAmazonSQS>();
            // Setup default behaviors for mockSqs if needed, e.g.:
            // mockSqs.Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            //        .ReturnsAsync(new SendMessageResponse { MessageId = "test-message-id" });
            ConfigureMockSqs?.Invoke(mockSqs); // Allow test-specific setup
            services.AddSingleton<IAmazonSQS>(mockSqs.Object);


            // (Optional) Mock IAmazonDynamoDB and IDynamoDBContext
            /*
            services.RemoveAll<IAmazonDynamoDB>();
            services.RemoveAll<IDynamoDBContext>();

            var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
            // ConfigureMockDynamoDB?.Invoke(mockDynamoDbClient);
            services.AddSingleton<IAmazonDynamoDB>(mockDynamoDbClient.Object);

            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            // Setup mockDynamoDbContext as needed
            // e.g., mockDynamoDbContext.Setup(c => c.LoadAsync<MyEntity>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            //                         .ReturnsAsync(new MyEntity());
            services.AddScoped<IDynamoDBContext>(sp => mockDynamoDbContext.Object);
            */

            // If SqsListenerForTestingService has external dependencies you want to avoid in *these* tests,
            // you might remove it or replace it with a dummy.
            // However, for DI tests, you usually want to ensure it *can* be constructed.
            // If it makes actual SQS calls in its constructor or StartAsync, that's an issue.
            // For now, let's assume it's fine for DI tests.
        });
    }
}