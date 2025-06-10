namespace ExpenseLoggerApi.Constants;

public static class SpreadsheetConstants
{
    public const int DataStartRow = 2;

    public static class Column
    {
        public const string Date = "B";
        public const string Description = "C";
        public const string Category = "D";
        public const string ExchangeRate = "E";
        public const string Amount = "F";
        public const string TotalFormula = "G";

        public const string DateCreated = "M";
        public const string Source = "O";
    }
}