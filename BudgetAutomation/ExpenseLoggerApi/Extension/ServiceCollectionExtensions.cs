using System.Threading.RateLimiting;
using Amazon.Lambda.Serialization.SystemTextJson;
using ExpenseLoggerApi.AotTypes;
using ExpenseLoggerApi.Interface;
using ExpenseLoggerApi.Service;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;

namespace ExpenseLoggerApi.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectSpecificServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddLogging(builder => builder.AddLambdaLogger());

        services.Configure<ExpenseLoggerSettings>(config.GetSection(ExpenseLoggerSettings.Configuration));
        services.AddSingleton<IValidateOptions<ExpenseLoggerSettings>, ExpenseLoggerSettingsValidator>();

        // TODO: fix Rate Limiting on lambda
        // Add Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                // Get settings via IOptions within the rate limiter setup
                var settings = httpContext.RequestServices.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

                if (!int.TryParse(settings.maxDailyRequest, out var rateLimit))
                {
                    rateLimit = 20; // Default if parsing fails
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    settings.googleApiKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimit,
                        Window = TimeSpan.FromDays(1),
                    });
            });
            // Added OnRejected handler for better debugging
            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.StatusCode = 429; // Too Many Requests
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Rate limit reached for request to {Path}.", context.HttpContext.Request.Path);
                return new ValueTask();
            };
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

#pragma warning disable IL2026
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
            options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
#pragma warning restore IL2026


        // Register Services
        services.AddScoped<SpreadsheetService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddSingleton<GoogleSheetsClientFactory>();
        services.AddSingleton<SheetsService>(sp =>
        {
            var factory = sp.GetRequiredService<GoogleSheetsClientFactory>();
            var settings = sp.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

            return factory.CreateSheetsService(settings.credentials);
        });
        services.AddSingleton<ISheetsDataAccessor, GoogleSheetsDataAccessor>();
        services.AddScoped<ExpenseLoggerService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

            return new ExpenseLoggerService(
                sp.GetRequiredService<ISheetsDataAccessor>(),
                sp.GetRequiredService<ICategoryService>(),
                sp.GetRequiredService<ILogger<ExpenseLoggerService>>()
            );
        });

        return services;
    }
}