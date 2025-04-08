using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using Moq;
using Telegram.Bot.Types;
using Xunit;

namespace BudgetBotTelegram.Tests.UnitTest.Handler.Command;

public class LogCommandTests
{
    private readonly Mock<ISenderGateway> _mockSenderGateway;
    private readonly Mock<IExpenseLoggerApiClient> _mockExpenseApiClient;
    private readonly LogCommand _logCommand;

    public LogCommandTests()
    {
        _mockSenderGateway = new Mock<ISenderGateway>();
        _mockExpenseApiClient = new Mock<IExpenseLoggerApiClient>();
        _logCommand = new LogCommand(_mockSenderGateway.Object, _mockExpenseApiClient.Object);
    }

    private void MockSender(Chat chat, string expectedReplyText, string? expectedLogMessage = null)
    {
        _mockSenderGateway
            .Setup(x => x.ReplyAsync(chat, expectedReplyText,
                expectedLogMessage ?? "Logged expense.",
                It.IsAny<Telegram.Bot.Types.Enums.ParseMode>(), It.IsAny<ReplyParameters?>(),
                It.IsAny<Telegram.Bot.Types.ReplyMarkups.ReplyMarkup?>(), It.IsAny<LinkPreviewOptions?>(),
                It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>?>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<bool>(), CancellationToken.None))
            .ReturnsAsync(new Message()); // Return a dummy message
    }

    [Theory]
    [InlineData("/log Groceries 50.50 Food", "Groceries", "50.50", "Food")]
    [InlineData("/log Groceries 50.50", "Groceries", "50.50", "")]
    [InlineData("log Groceries 50.50 Food", "Groceries", "50.50", "Food")]
    [InlineData("log Groceries 50.50", "Groceries", "50.50", "")]
    [InlineData("/log This is the description 13 Cat", "This is the description", "13", "Cat")]
    [InlineData("/log This is the description 13", "This is the description", "13", "")]
    [InlineData("/log 99 11 Cat", "99", "11", "Cat")]
    [InlineData("/log 99 11", "99", "11", "")]
    [InlineData("log 99 long description 11234,60 Cat", "99 long description", "11234,60", "Cat")]
    [InlineData("log 99 long description 11234,60", "99 long description", "11234,60", "")]
    public async Task HandleLogAsync_ValidateExpenseCreation(string logMessage, string expectedDescription, string expectedAmount,
        string expectedCategory)
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = logMessage
        };
        var cancellationToken = CancellationToken.None;
        var expectedExpense = new Expense
            { Description = expectedDescription, Amount = expectedAmount, Category = expectedCategory };
        var expectedReplyText = $"Logged Expense\n{expectedExpense}";
        Expense? capturedExpense = null; // Variable to capture the expense

        _mockExpenseApiClient
            .Setup(x => x.LogExpenseAsync(It.IsAny<Expense>(), cancellationToken)) // Match any Expense object
            .Callback<Expense, CancellationToken>((expense, ct) => capturedExpense = expense) // Capture the Expense object
            .Returns(Task.CompletedTask);

        MockSender(message.Chat, expectedReplyText);

        // Act
        await _logCommand.HandleLogAsync(message, cancellationToken);

        // Assert
        _mockExpenseApiClient.Verify(x => x.LogExpenseAsync(It.IsAny<Expense>(), cancellationToken),
            Times.Once); // Verify the method was called once
        _mockSenderGateway.VerifyAll();

        // Assert on the captured expense
        Assert.NotNull(capturedExpense);
        Assert.Equal(expectedExpense.Description, capturedExpense.Description);
        Assert.Equal(expectedExpense.Amount, capturedExpense.Amount);
        Assert.Equal(expectedExpense.Category, capturedExpense.Category);
    }

    [Fact]
    public async Task HandleLogAsync_ValidMessageWithCategory_LogsExpenseAndReplies()
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = "/log Groceries 50.50 Food"
        };
        var cancellationToken = CancellationToken.None;
        var expectedExpense = new Expense { Description = "Groceries", Amount = "50.50", Category = "Food" };
        var expectedReplyText = $"Logged Expense\n{expectedExpense}";
        Expense? capturedExpense = null; // Variable to capture the expense

        _mockExpenseApiClient
            .Setup(x => x.LogExpenseAsync(It.IsAny<Expense>(), cancellationToken)) // Match any Expense object
            .Callback<Expense, CancellationToken>((expense, ct) => capturedExpense = expense) // Capture the Expense object
            .Returns(Task.CompletedTask);

        MockSender(message.Chat, expectedReplyText);

        // Act
        await _logCommand.HandleLogAsync(message, cancellationToken);

        // Assert
        _mockExpenseApiClient.Verify(x => x.LogExpenseAsync(It.IsAny<Expense>(), cancellationToken),
            Times.Once); // Verify the method was called once

        _mockSenderGateway.VerifyAll();

        // Assert on the captured expense
        Assert.NotNull(capturedExpense);
        Assert.Equal(expectedExpense.Description, capturedExpense.Description);
        Assert.Equal(expectedExpense.Amount, capturedExpense.Amount);
        Assert.Equal(expectedExpense.Category, capturedExpense.Category);
    }

    [Fact]
    public async Task HandleLogAsync_ValidMessageWithoutCategory_LogsExpenseAndReplies()
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = "log Rent 1200"
        };
        var cancellationToken = CancellationToken.None;
        var expectedExpense = new Expense { Description = "Rent", Amount = "1200", Category = "" };
        var expectedReplyText = $"Logged Expense\n{expectedExpense}";

        _mockExpenseApiClient
            .Setup(x => x.LogExpenseAsync(It.Is<Expense>(e =>
                e.Description == expectedExpense.Description &&
                e.Amount == expectedExpense.Amount &&
                e.Category == expectedExpense.Category), cancellationToken))
            .Returns(Task.CompletedTask);

        MockSender(message.Chat, expectedReplyText);

        // Act
        await _logCommand.HandleLogAsync(message, cancellationToken);

        // Assert
        _mockExpenseApiClient.VerifyAll();
        _mockSenderGateway.VerifyAll();
    }

    [Fact]
    public async Task HandleLogAsync_InvalidAmountFormat_RepliesWithError()
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = "/log Coffee abc"
        };
        var cancellationToken = CancellationToken.None;
        var expectedReplyText = "Invalid message format for logging expense.";

        MockSender(message.Chat, expectedReplyText, ""); // TODO: change log on invalid amount

        // Act
        await _logCommand.HandleLogAsync(message, cancellationToken);

        // Assert
        _mockSenderGateway.VerifyAll();
        _mockExpenseApiClient.Verify(x => x.LogExpenseAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()),
            Times.Never); // Ensure LogExpenseAsync was not called
    }

    [Fact]
    public async Task HandleLogAsync_ExpenseApiThrowsArgumentException_RepliesWithErrorMessage()
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = "/log Test 10"
        };
        var cancellationToken = CancellationToken.None;
        var expectedExceptionMessage = "API Error";
        var apiException = new ArgumentException(expectedExceptionMessage);
        var expectedReplyText = expectedExceptionMessage;
        var expectedLogMessage = $"Argument Exception: {expectedExceptionMessage}.";

        _mockExpenseApiClient
            .Setup(x => x.LogExpenseAsync(It.IsAny<Expense>(), cancellationToken))
            .ThrowsAsync(apiException);

        MockSender(message.Chat, expectedReplyText, expectedLogMessage);

        // Act
        await _logCommand.HandleLogAsync(message, cancellationToken);

        // Assert
        _mockExpenseApiClient.VerifyAll();
        _mockSenderGateway.VerifyAll();
    }

    [Theory]
    [InlineData("/log Invalid")] // Too few arguments
    [InlineData("Invalid")] // Too few arguments (no slash)
    [InlineData("log 10")] // Too few arguments (no slash)
    [InlineData("log Invalid")] // Too few arguments (no slash)
    public async Task HandleLogAsync_InvalidMessageFormat_ThrowsArgumentException(string messageText)
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = messageText
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _logCommand.HandleLogAsync(message, cancellationToken));

        MockSender(message.Chat, string.Empty);

        _mockExpenseApiClient.Verify(x => x.LogExpenseAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleLogAsync_NullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        Message? message = null;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _logCommand.HandleLogAsync(message!, cancellationToken));
    }

    [Fact]
    public async Task HandleLogAsync_NullMessageText_ThrowsArgumentNullException()
    {
        // Arrange
        var message = new Message { Chat = new Chat { Id = 123 }, Text = null }; // Text is null
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _logCommand.HandleLogAsync(message, cancellationToken));
    }

    [Fact]
    public async Task HandleLogAsync_MessageNotStartingWithLog_ThrowsArgumentException()
    {
        // Arrange
        var message = new Message { Chat = new Chat { Id = 123 }, Text = "hello world" };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _logCommand.HandleLogAsync(message, cancellationToken));
        Assert.Contains("doesn't start with log command", exception.Message);
    }
}