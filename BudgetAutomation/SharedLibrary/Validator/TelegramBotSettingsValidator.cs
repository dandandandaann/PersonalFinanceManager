using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

namespace SharedLibrary.Validator;

public class TelegramBotSettingsValidator : IValidateOptions<TelegramBotSettings>
{
    public ValidateOptionsResult Validate(string? name, TelegramBotSettings options)
    {
        var failures = new List<string>();

        if (options == null!)
        {
            failures.Add($"{nameof(options)} is missing in configuration.");
            return ValidateOptionsResult.Fail(failures);
        }

        if (string.IsNullOrWhiteSpace(options.Token))
        {
            failures.Add($"{nameof(options.Token)} is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            failures.Add($"{nameof(options.Name)} is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Handle))
        {
            failures.Add($"{nameof(options.Handle)} is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(options.WebhookToken))
        {
            failures.Add($"{nameof(options.WebhookToken)} is missing or empty.");
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}