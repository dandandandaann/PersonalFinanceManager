namespace ExpenseLoggerApi.Model;

public class ResponseModel
{
    public bool Success { get; set; }
    public Expense? expense { get; set; }
}