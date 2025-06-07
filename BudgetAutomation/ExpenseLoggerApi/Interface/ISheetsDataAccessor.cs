using Google.Apis.Sheets.v4.Data;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace ExpenseLoggerApi.Interface
{
    /// <summary>
    /// Defines operations for accessing and modifying data within a Google Sheet.
    /// </summary>
    public interface ISheetsDataAccessor
    {
        /// <summary>
        /// Gets the numeric ID of a sheet within a spreadsheet by its title.
        /// </summary>
        Task<int> GetSheetIdByNameAsync(string spreadsheetId, string sheetName);

        /// <summary>
        /// Finds the first row index in a specific column that is empty or whitespace, starting from a given row.
        /// </summary>
        Task<int> FindFirstEmptyRowAsync(string spreadsheetId, string sheetName, string column, int startRow);
        Task<int> FindLastItemAsync(string spreadsheetId, string sheetName, string column, int startRow);

        /// <summary>
        /// Inserts a new empty row at the specified index within a sheet.
        /// </summary>
        Task InsertRowAsync(string spreadsheetId, int sheetId, int rowIndex);
        Task DeleteRowAsync(string spreadsheetId, int sheetId, int rowIndex);
        Task<IList<object>> ReadRowValuesAsync(string spreadsheetId, string sheetName, int rowIndex);


        /// <summary>
        /// Performs a batch update of cell values in a spreadsheet.
        /// </summary>
        Task BatchUpdateValuesAsync(string spreadsheetId, BatchUpdateValuesRequest request);

        /// <summary>
        /// Check if we have access to the Spreadsheet
        /// </summary>
        Task<SpreadsheetValidationResponse> ValidateSpreadsheetIdAsync(SpreadsheetValidationRequest request);
    }
}