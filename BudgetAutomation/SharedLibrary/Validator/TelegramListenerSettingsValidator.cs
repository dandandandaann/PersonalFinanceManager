using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

namespace SharedLibrary.Validator;

public class TelegramListenerSettingsValidator : IValidateOptions<TelegramListenerSettings>
{
    public ValidateOptionsResult Validate(string? name, TelegramListenerSettings options)
    {
        var failures = new List<string>();

        if (options == null!)
        {
            failures.Add($"{nameof(options)} is missing in configuration.");
            return ValidateOptionsResult.Fail(failures);
        }

        if (string.IsNullOrWhiteSpace(options.TelegramUpdateQueue))
        {
            failures.Add($"{nameof(options.TelegramUpdateQueue)} is missing or empty.");
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}