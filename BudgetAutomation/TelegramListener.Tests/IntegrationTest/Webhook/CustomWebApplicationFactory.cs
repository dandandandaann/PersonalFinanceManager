// using BudgetAutomation.Engine.Interface;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Moq;
//
// namespace BudgetAutomation.Engine.Tests.IntegrationTest.Webhook
// {
//     public class CustomWebApplicationFactory : WebApplicationFactory<Other.Program>
//     {
//         public string TestWebhookToken { get; set; } = "WebhookToken";
//         public Mock<IUpdateHandler> MockUpdateHandler { get; } = new();
//
//         // Constructor without ITestOutputHelper
//         public CustomWebApplicationFactory()
//         {
//             // ClientOptions.PreserveBaseAddress = true;
//             ClientOptions.AllowAutoRedirect = false;
//             ClientOptions.HandleCookies = true;
//         }
//
//         protected override void ConfigureWebHost(IWebHostBuilder builder)
//         {
//             builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
//             builder.UseSetting("ASPNETCORE_DETAILEDERRORS", "true");
//
//             builder.ConfigureAppConfiguration((context, conf) =>
//             {
//                 conf.AddInMemoryCollection(new Dictionary<string, string?>
//                 {
//                     ["BotConfiguration:WebhookToken"] = TestWebhookToken,
//                     ["BotConfiguration:Token"] = "dummy-telegram-api-token",
//                     ["SpreadsheetManagerApi:Url"] = "http://localhost:9999"
//                 });
//             });
//
//             builder.ConfigureServices(services =>
//             {
//                 // Remove IUpdateHandler
//                 var updateHandlerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUpdateHandler));
//                 if (updateHandlerDescriptor != null)
//                 {
//                     services.Remove(updateHandlerDescriptor);
//                 }
//
//                 // Add Mock
//                 services.AddScoped(_ => MockUpdateHandler.Object);
//
//                 // Remove Hosted Service
//                 var hostedServiceDescriptor = services.SingleOrDefault(
//                     d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(ConfigureWebhook));
//                 if (hostedServiceDescriptor != null)
//                 {
//                     services.Remove(hostedServiceDescriptor);
//                 }
//             });
//         }
//
//         public static async Task<string> GetContent(HttpResponseMessage response)
//         {
//             if (response?.Content == null) return "[No Response Content]";
//             try
//             {
//                 return await response.Content.ReadAsStringAsync();
//             }
//             catch (Exception ex)
//             {
//                 return $"[Error reading response content: {ex.Message}]";
//             }
//         }
//     }
// }