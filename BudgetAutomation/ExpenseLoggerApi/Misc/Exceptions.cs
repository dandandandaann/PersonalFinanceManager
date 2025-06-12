namespace ExpenseLoggerApi.Misc;

public class SheetNotFoundException(string message) : Exception(message);

public class SpreadsheetNotFoundException(string message) : Exception(message);
