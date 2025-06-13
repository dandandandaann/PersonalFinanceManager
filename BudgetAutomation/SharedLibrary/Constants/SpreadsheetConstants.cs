namespace SharedLibrary.Constants;

public static class SpreadsheetConstants
{
    public const double DateTimeZone = -3;

    public static class TransactionColumn
    {
        public const int DataStartRow = 2;

        public const string Date = "B";
        public const string Description = "C";
        public const string Category = "D";
        public const string ExchangeRate = "E";
        public const string Amount = "F";
        public const string TotalFormula = "G";

        public const string DateCreated = "M";
        public const string Source = "O";
    }

    public static class CategoryColumn
    {
        public const int DataStartRow = 2;

        public const string Description = "A";
    }

    public static class CategorizadorColumn
    {
        public const int DataStartRow = 2;

        public const string Category = "A";
        public const string DescriptionPattern = "B";
    }

    public static class Sheets
    {
        public const string Transactions = "Transações";
        public const string Categories = "Categorias";
        public const string Categorizer = "Categorizador";
    }

}