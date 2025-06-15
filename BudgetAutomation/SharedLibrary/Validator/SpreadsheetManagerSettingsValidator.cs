using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

namespace SharedLibrary.Validator;

public class SpreadsheetManagerSettingsValidator : IValidateOptions<SpreadsheetManagerApiSettings>
{
    public ValidateOptionsResult Validate(string? name, SpreadsheetManagerApiSettings options)
    {
        var failures = new List<string>();

        if (options == null!)
        {
            failures.Add($"{nameof(options)} is missing in configuration.");
            return ValidateOptionsResult.Fail(failures);
        }

        if (string.IsNullOrWhiteSpace(options.googleApiKey))
        {
            failures.Add($"{nameof(options.googleApiKey)} is missing or empty.");
        }
        if (string.IsNullOrWhiteSpace(options.credentials))
        {
            failures.Add($"{nameof(options.credentials)} is missing or empty.");
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}