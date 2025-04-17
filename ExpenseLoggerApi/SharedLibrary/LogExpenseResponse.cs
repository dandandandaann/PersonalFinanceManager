namespace SharedLibrary;

public class LogExpenseResponse
{
    public bool Success { get; set; }
    public Expense? expense { get; set; }
}