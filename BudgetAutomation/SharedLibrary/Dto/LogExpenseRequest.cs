namespace SharedLibrary.Dto
{
    public class LogExpenseRequest
    {
        public string SpreadsheetId { get; set; }
        public string Description { get; set; }
        public string Amount { get; set; }
        public string? Category { get; set;}
    }
}