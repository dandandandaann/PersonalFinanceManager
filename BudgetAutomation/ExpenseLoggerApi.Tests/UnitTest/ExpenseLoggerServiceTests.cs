using SharedLibrary.Model;

namespace UnitTest.UnitTest;

public class ExpenseLoggerServiceTests : IDisposable
{
    private readonly Mock<ISheetsDataAccessor> _mockSheetsAccessor;
    private readonly Mock<ILogger<ExpenseLoggerService>> _mockLogger;
    private const string SpreadsheetId = "test-spreadsheet-id";
    private readonly ExpenseLoggerService _service;
    private readonly string _expectedSheetName;

    // Consider using TimeProvider for more robust date testing if needed
    // private readonly TimeProvider _frozenTimeProvider;

    public ExpenseLoggerServiceTests()
    {
        _mockSheetsAccessor = new Mock<ISheetsDataAccessor>();
        _mockLogger = new Mock<ILogger<ExpenseLoggerService>>();

        List<Category> categories =
        [
            new() { Name = "Groceries", Alias = ["food", "supermarket"] },
            new() { Name = "Utilities", Alias = ["bills"] },
            new() { Name = "Transport" } // No aliases
        ];

        _expectedSheetName = DateTime.Now.ToString("MM-yyyy");

        _service = new ExpenseLoggerService(
            _mockSheetsAccessor.Object,
            categories,
            _mockLogger.Object
        );
    }

    // Implement IDisposable if freezing time
    public void Dispose()
    {
        // TimeProvider.Reset() or similar cleanup if needed
    }

    private void SetupSuccessfulSheetsApiFlow(int expectedRow, int expectedSheetId = 12345)
    {
        _mockSheetsAccessor.Setup(s => s.GetSheetIdByNameAsync(SpreadsheetId, _expectedSheetName))
            .ReturnsAsync(expectedSheetId);

        _mockSheetsAccessor.Setup(s => s.FindFirstEmptyRowAsync(SpreadsheetId, _expectedSheetName, "B", 15))
            .ReturnsAsync(expectedRow);

        _mockSheetsAccessor.Setup(s => s.InsertRowAsync(SpreadsheetId, expectedSheetId, expectedRow))
            .Returns(Task.CompletedTask); // Or ReturnsAsync(someResponse) if needed

        _mockSheetsAccessor.Setup(s => s.BatchUpdateValuesAsync(SpreadsheetId, It.IsAny<BatchUpdateValuesRequest>()))
            .Returns(Task.CompletedTask); // Or ReturnsAsync(someResponse) if needed
    }

    // --- Happy Path Tests ---

    [Theory]
    [InlineData("123,45", "123,45")]
    [InlineData("123.45", "123,45")]
    [InlineData("123", "123,00")]
    [InlineData("0.45", "0,45")]
    [InlineData("0,45", "0,45")]
    public async Task LogExpense_ValidInput_ShouldLogExpenseAndReturnCorrectObject(string amount, string expectedAmount)
    {
        // Arrange
        var description = "Test Item";
        var categoryInput = "Groceries";
        var expectedCategory = "Groceries";
        var expectedRow = 20;
        var expectedSheetId = 54321;

        SetupSuccessfulSheetsApiFlow(expectedRow, expectedSheetId);

        // Act
        var result = await _service.LogExpense(SpreadsheetId, description, amount, categoryInput);

        // Assert
        result.ShouldNotBeNull();
        result.Description.ShouldBe(description);
        result.Amount.ShouldBe(expectedAmount);
        result.Category.ShouldBe(expectedCategory);

        // Verify Sheets Accessor Calls
        _mockSheetsAccessor.Verify(s => s.GetSheetIdByNameAsync(SpreadsheetId, _expectedSheetName), Times.Once);
        _mockSheetsAccessor.Verify(s => s.FindFirstEmptyRowAsync(SpreadsheetId, _expectedSheetName, "B", 15), Times.Once);
        _mockSheetsAccessor.Verify(s => s.InsertRowAsync(SpreadsheetId, expectedSheetId, expectedRow), Times.Once);

        // Verify Batch Update Content
        _mockSheetsAccessor.Verify(s => s.BatchUpdateValuesAsync(
            SpreadsheetId,
            It.Is<BatchUpdateValuesRequest>(req =>
                req.ValueInputOption == "USER_ENTERED" &&
                req.Data.Count == 4 &&
                req.Data[0].Range == $"{_expectedSheetName}!B{expectedRow}" &&
                (string)req.Data[0].Values[0][0] == description &&
                req.Data[1].Range == $"{_expectedSheetName}!E{expectedRow}" &&
                (string)req.Data[1].Values[0][0] == expectedCategory &&
                req.Data[2].Range == $"{_expectedSheetName}!H{expectedRow}" &&
                // (string)req.Data[2].Values[0][0] == expectedAmount &&
                req.Data[3].Range == $"{_expectedSheetName}!I{expectedRow}" && (string)req.Data[3].Values[0][0] ==
                $"=IF(ISBLANK(H{expectedRow}); 0; IF(ISBLANK(F{expectedRow}); H{expectedRow}; F{expectedRow}*H{expectedRow}))"
            )), Times.Once);
    }

    [Theory]
    [InlineData("Groceries", "Groceries")]
    [InlineData("food", "Groceries")]
    [InlineData("NonExistent", "")]
    [InlineData("", "")]
    [InlineData("BiLlS", "Utilities")]
    public async Task LogExpense_CategoryAliasUsed_ShouldResolveToCorrectCategoryName(
        string categoryInput,
        string expectedCategory)
    {
        // Arrange
        var description = "Electricity Bill";
        var amount = "50.00";

        var expectedRow = 30;

        SetupSuccessfulSheetsApiFlow(expectedRow);

        // Act
        var result = await _service.LogExpense(SpreadsheetId, description, amount, categoryInput);

        // Assert
        result.ShouldNotBeNull();
        result.Category.ShouldBe(expectedCategory);

        _mockSheetsAccessor.Verify(s => s.BatchUpdateValuesAsync(
                SpreadsheetId,
                It.Is<BatchUpdateValuesRequest>(req => (string)req.Data[1].Values[0][0] == expectedCategory)),
            Times.Once);
    }

    // --- Error Handling Tests ---

    [Theory]
    [InlineData("abc")]
    [InlineData("12..34")]
    [InlineData("12,34,56")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task LogExpense_InvalidAmountFormat_ShouldThrowArgumentExceptionAndLog(string invalidAmount)
    {
        // Arrange
        var description = "Invalid Purchase";
        var categoryInput = "Groceries";

        // Act
        async Task Action() => await _service.LogExpense(SpreadsheetId, description, invalidAmount, categoryInput);

        // Assert
        var exception = await Should.ThrowAsync<ArgumentException>((Func<Task>)Action);
        exception.Message.ShouldStartWith("Invalid amount format."); // Check start of message
        exception.ParamName.ShouldBe("amount"); // Check parameter name

        // Verify logger was called for the error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Invalid amount format received: '{invalidAmount}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify no sheet interactions occurred
        _mockSheetsAccessor.Verify(s => s.GetSheetIdByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockSheetsAccessor.Verify(
            s => s.FindFirstEmptyRowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
        _mockSheetsAccessor.Verify(s => s.InsertRowAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockSheetsAccessor.Verify(s => s.BatchUpdateValuesAsync(It.IsAny<string>(), It.IsAny<BatchUpdateValuesRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task LogExpense_SheetsApiThrowsException_ShouldLogAndRethrow()
    {
        // Arrange
        var description = "Failed Item";
        var amount = "10.0";
        var categoryInput = "Utilities";
        var expectedException = new Exception("Simulated Google Sheets API error");

        // Setup mock to throw on the first call
        _mockSheetsAccessor.Setup(s => s.GetSheetIdByNameAsync(SpreadsheetId, _expectedSheetName))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> action = async () => await _service.LogExpense(SpreadsheetId, description, amount, categoryInput);

        // Assert
        var exception = await Should.ThrowAsync<Exception>(action);
        exception.Message.ShouldBe(expectedException.Message); // Should rethrow the original exception

        // Verify logger was called for the error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to log expense for description '{description}'")),
                expectedException, // Verify the specific exception was logged
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify subsequent sheet interactions did not occur
        _mockSheetsAccessor.Verify(
            s => s.FindFirstEmptyRowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
        _mockSheetsAccessor.Verify(s => s.InsertRowAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockSheetsAccessor.Verify(s => s.BatchUpdateValuesAsync(It.IsAny<string>(), It.IsAny<BatchUpdateValuesRequest>()),
            Times.Never);
    }
    // Add more tests for exceptions thrown by other SheetsAccessor methods if needed
    // e.g., FindFirstEmptyRowAsync, InsertRowAsync, BatchUpdateValuesAsync throwing
}