using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

namespace SharedLibrary.Validator;

public class SpreadsheetManagerApiClientSettingsValidator : IValidateOptions<SpreadsheetManagerApiClientSettings>
{
    public ValidateOptionsResult Validate(string? name, SpreadsheetManagerApiClientSettings options)
    {
        var failures = new List<string>();

        if (options == null!)
        {
            failures.Add($"{nameof(options)} is missing in configuration.");
            return ValidateOptionsResult.Fail(failures);
        }

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            failures.Add($"{nameof(options.Key)} is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Url))
        {
            failures.Add($"{nameof(options.Url)} is missing or empty.");
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}