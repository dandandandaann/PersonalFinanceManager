using System.Text.RegularExpressions;
using SharedLibrary.Constants;
using SharedLibrary.Telegram.Types.ReplyMarkups;

namespace BudgetAutomation.Engine.Misc;

public static class Utility
{
    public static bool TryExtractCommandArguments(
        string text, string commandName, out string arguments, Func<Regex>? validationRegex = null
    )
    {
        arguments = "";
        var commandWithSlash = "/" + commandName;
        var prefixLength = 0;
        var startsWithCommand = true;

        // Check if it starts with "/command" (case-insensitive)
        if (text.StartsWith(commandWithSlash, StringComparison.OrdinalIgnoreCase))
        {
            prefixLength = commandWithSlash.Length;
        }
        // Check if it starts with "command" (case-insensitive)
        else if (text.StartsWith(commandName, StringComparison.OrdinalIgnoreCase))
        {
            prefixLength = commandName.Length;
        }
        else
        {
            // Doesn't start with the command, so it's only arguments
            startsWithCommand = false;
            arguments = text;
        }

        if (startsWithCommand)
        {
            // The message is exactly the command without arguments (e.g., "/log" or "log")
            if (text.Length == prefixLength)
            {
                arguments = string.Empty;
                return true;
            }

            // This is not the command (e.g. "/logging" is not "/log")
            if (text[prefixLength] != ' ')
                return false;

            arguments = text.Substring(prefixLength + 1).Trim();
        }

        if (validationRegex != null)
            return validationRegex().IsMatch(arguments);

        return true;
    }

    public static string GetGreetingByTimeOfDay()
    {
        var utcNow = DateTime.UtcNow;
        var offset = TimeSpan.FromHours(SpreadsheetConstants.DateTimeZone);
        var localTime = utcNow + offset;
        var hour = localTime.Hour;

        return hour switch
        {
            >= 6 and < 12 => "Bom dia",
            >= 12 and < 18 => "Boa tarde",
            _ => "Boa noite"
        };
    }

    public static InlineKeyboardButton Button(string text, string command, object? argument = null)
    {
        return InlineKeyboardButton.WithCallbackData(text, $"/{command} {argument}");
    }
}