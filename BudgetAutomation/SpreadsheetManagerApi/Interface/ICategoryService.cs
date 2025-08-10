using SharedLibrary.Dto;
using System.Collections.Generic;

namespace SpreadsheetManagerApi.Interface;

public interface ICategoryService
{
    Task<string> DecideCategoryAsync(string spreadsheetId, string userCategory, string description);
    Task<AddCategoryRuleResponse> AddCategoryRuleAsync(string spreadsheetId, string category, string descriptionPattern);
    Task<IList<string>> GetAllCategoriesAsync(string spreadsheetId);
} 