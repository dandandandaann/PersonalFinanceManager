using System.Text.RegularExpressions;

namespace BudgetBotTelegram.Misc;

public static class Utility
{
    public static bool TryExtractCommandArguments(
        string text, string commandName, Func<Regex>? validationRegex, out string arguments
    )
    {
        arguments = "";
        var commandWithSlash = "/" + commandName;
        var prefixLength = -1;

        // Check if it starts with "/command" (case-insensitive)
        if (text.StartsWith(commandWithSlash, StringComparison.OrdinalIgnoreCase))
        {
            prefixLength = commandWithSlash.Length;
        }
        else
        {
            // Doesn't start with the command at all
            return false;
        }

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

        if (validationRegex != null)
            return validationRegex().IsMatch(arguments);

        return true;

    }
}