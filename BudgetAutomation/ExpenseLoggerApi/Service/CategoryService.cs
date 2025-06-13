using ExpenseLoggerApi.Interface;
using SharedLibrary.Constants;

namespace ExpenseLoggerApi.Service;

public class CategoryService(ISheetsDataAccessor sheetsAccessor, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<string> DecideCategoryAsync(string spreadsheetId, string userCategory, string description)
    {
        description = description.Trim().Normalize();

        // If user provided a category, validate it exists
        if (!string.IsNullOrEmpty(userCategory))
        {
            var categories = await GetCategoriesAsync(spreadsheetId);

            var matchedCategory = categories.FirstOrDefault(c => string.Equals(c, userCategory, StringComparison.OrdinalIgnoreCase));
            if (matchedCategory != null)
            {
                return matchedCategory;
            }
        }

        // If no user category or invalid, try to match from description
        var allCategories = await GetCategoriesAsync(spreadsheetId);
        foreach (var category in allCategories)
        {
            if (description.Contains(category, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        return string.Empty;
    }

    private async Task<IList<string>> GetCategoriesAsync(string spreadsheetId)
    {
        try
        {
            return await sheetsAccessor.ReadColumnValuesAsync(
                spreadsheetId,
                SpreadsheetConstants.Sheets.Categories,
                SpreadsheetConstants.CategoryColumn.Description,
                SpreadsheetConstants.CategoryColumn.DataStartRow
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve categories from spreadsheet {SpreadsheetId}", spreadsheetId);
            throw;
        }
    }
} 