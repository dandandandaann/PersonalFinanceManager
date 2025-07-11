using SpreadsheetManagerApi.Interface;
using SharedLibrary.Constants;
using SharedLibrary.Dto;

namespace SpreadsheetManagerApi.Service;

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
                SpreadsheetConstants.Categories.SheetName,
                SpreadsheetConstants.Categories.Column.Category,
                SpreadsheetConstants.Categories.DataStartRow
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
            var range = $"{SpreadsheetConstants.Categorizator.SheetName}!" +
                        $"{SpreadsheetConstants.Categorizator.Column.Category}{SpreadsheetConstants.Categorizator.DataStartRow}:" +
                        $"{SpreadsheetConstants.Categorizator.Column.DescriptionPattern}";

            var response = await sheetsAccessor.ReadColumnValuesAsync(
                spreadsheetId,
                SpreadsheetConstants.Categorizator.SheetName,
                SpreadsheetConstants.Categorizator.Column.Category,
                SpreadsheetConstants.Categorizator.DataStartRow);;

            var categories = response.ToList();

            // TODO: join both requests in 1 call
            var patterns = await sheetsAccessor.ReadColumnValuesAsync(
                spreadsheetId,
                SpreadsheetConstants.Categorizator.SheetName,
                SpreadsheetConstants.Categorizator.Column.DescriptionPattern,
                SpreadsheetConstants.Categorizator.DataStartRow
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

    public async Task<AddCategoryRuleResponse> AddCategoryRuleAsync(string spreadsheetId, string category, string descriptionPattern)
    {
        try
        {
            var sheetId = await sheetsAccessor.GetSheetIdByNameAsync(spreadsheetId, SpreadsheetConstants.Categorizator.SheetName);
            var nextRow = await sheetsAccessor.FindFirstEmptyRowAsync(
                spreadsheetId,
                SpreadsheetConstants.Categorizator.SheetName,
                SpreadsheetConstants.Categorizator.Column.Category,
                SpreadsheetConstants.Categorizator.DataStartRow);

            // Insert a new row at the next available position
            await sheetsAccessor.InsertRowAsync(spreadsheetId, sheetId, nextRow);

            // Prepare the values to write
            var range = $"{SpreadsheetConstants.Categorizator.SheetName}!{SpreadsheetConstants.Categorizator.Column.Category}{nextRow}:{SpreadsheetConstants.Categorizator.Column.DescriptionPattern}{nextRow}";
            var values = new List<IList<object>>
            {
                new List<object> { category, descriptionPattern }
            };
            var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
            {
                Range = range,
                Values = values
            };
            var updateRequest = new Google.Apis.Sheets.v4.Data.BatchUpdateValuesRequest
            {
                Data = new List<Google.Apis.Sheets.v4.Data.ValueRange> { valueRange },
                ValueInputOption = "RAW"
            };
            await sheetsAccessor.BatchUpdateValuesAsync(spreadsheetId, updateRequest);

            return new AddCategoryRuleResponse { Success = true, Message = "Category rule added successfully."};
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add category rule to spreadsheet {SpreadsheetId}", spreadsheetId);
            return new AddCategoryRuleResponse { Success = false, Message = "Failed to add category rule."};
        }
    }
} 