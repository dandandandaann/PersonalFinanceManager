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

            return string.Empty;
        }

        // If no user category, try to match from auto-categorization rules
        var autoCategories = await GetAutoCategoriesAsync(spreadsheetId);
        foreach (var (category, pattern) in autoCategories)
        {
            if (description.Contains(pattern, StringComparison.OrdinalIgnoreCase))
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

    private async Task<IList<(string Category, string Pattern)>> GetAutoCategoriesAsync(string spreadsheetId)
    {
        try
        {
            var range = $"{SpreadsheetConstants.Sheets.Categorizer}!" +
                        $"{SpreadsheetConstants.CategorizadorColumn.Category}{SpreadsheetConstants.CategorizadorColumn.DataStartRow}:" +
                        $"{SpreadsheetConstants.CategorizadorColumn.DescriptionPattern}";

            var response = await sheetsAccessor.ReadColumnValuesAsync(
                spreadsheetId,
                SpreadsheetConstants.Sheets.Categorizer,
                SpreadsheetConstants.CategorizadorColumn.Category,
                SpreadsheetConstants.CategorizadorColumn.DataStartRow);;

            var categories = response.ToList();

            // TODO: join both requests in 1 call
            var patterns = await sheetsAccessor.ReadColumnValuesAsync(
                spreadsheetId,
                SpreadsheetConstants.Sheets.Categorizer,
                SpreadsheetConstants.CategorizadorColumn.DescriptionPattern,
                SpreadsheetConstants.CategorizadorColumn.DataStartRow
            );

            return categories
                .Zip(patterns, (category, pattern) => (category, pattern))
                .Where(x => !string.IsNullOrWhiteSpace(x.category) && !string.IsNullOrWhiteSpace(x.pattern))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve auto-categorization rules from spreadsheet {SpreadsheetId}", spreadsheetId);
            throw;
        }
    }
} 