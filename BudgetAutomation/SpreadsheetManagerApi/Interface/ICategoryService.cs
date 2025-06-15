namespace SpreadsheetManagerApi.Interface;

public interface ICategoryService
{
    Task<string> DecideCategoryAsync(string spreadsheetId, string userCategory, string description);
} 