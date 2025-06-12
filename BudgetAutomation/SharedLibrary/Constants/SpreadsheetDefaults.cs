namespace SharedLibrary.Constants;

public static class SpreadsheetConstants
{
    public const int DataStartRow = 2;
    public const double DateTimeZone = -3;

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

    public static class Sheets
    {
        public const string Transactions = "Transações";
        public const string Categories = "Categorias";
    }

}