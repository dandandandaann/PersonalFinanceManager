namespace SharedLibrary.Constants;

public static class SpreadsheetConstants
{
    public const double DateTimeZone = -3;
    public const string TemplateUrl = "https://docs.google.com/spreadsheets/d/1BLgqw1RPAX2_Qv6w1blPW2L5VTXrfFvWS0S2MDHWqfw/";

    public static class Categorizator
    {
        public const string SheetName = "Categorizador";
        public const int DataStartRow = 2;

        public static class Column
        {
            public const string Category = "A";
            public const string DescriptionPattern = "B";
        }
    }

    public static class Categories
    {
        public const string SheetName = "Categorias";
        public const int DataStartRow = 2;

        public static class Column
        {
            public const string Category = "A";
        }
    }

    public static class Transactions
    {
        public const string SheetName = "Transações";
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

}