using System.Text;

namespace ExpenseLoggerApi;

public static class JsonHandler
{
    public static string ToJson(Dictionary<string, object> dict, int indentLevel = 0)
    {
        var sb = new StringBuilder();
        string indent = new string(' ', indentLevel * 4);
        sb.AppendLine("{");

        foreach (var kvp in dict)
        {
            string? value = kvp.Value is string ? $"\"{kvp.Value}\"" : kvp.Value.ToString();
            sb.AppendLine($"{indent}    \"{kvp.Key}\": {value},");
        }

        if (dict.Count > 0)
            sb.Length--; // Remove the last comma

        sb.AppendLine($"{indent}}}");
        return sb.ToString();
    }
}