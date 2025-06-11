using SharedLibrary.Model;

namespace ExpenseLoggerApi.Misc;

public static class Utility
{
    public static string DecideCategory(string userCategory, string description, IEnumerable<Category> categories)
    {
        description = description.Trim().Normalize();

        if (string.IsNullOrEmpty(userCategory))
        {
            foreach (var category in categories)
            {
                if (category.Alias == null)
                    continue;

                if (category.Alias.Any(alias =>
                        description.Contains(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    return category.Name;
                }
            }

            return "";
        }

        foreach (var category in categories)
        {
            if (!string.IsNullOrEmpty(userCategory))
            {
                if (category.Name.Equals(userCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return category.Name;
                }

                if (category.Alias != null && category.Alias.Any(alias =>
                        alias.Equals(userCategory, StringComparison.OrdinalIgnoreCase)))
                {
                    return category.Name;
                }
            }
        }

        return ""; // Return empty string if no match found
    }
    
}