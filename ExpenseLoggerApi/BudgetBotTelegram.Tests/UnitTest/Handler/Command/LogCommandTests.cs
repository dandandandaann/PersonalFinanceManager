using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using Microsoft.Extensions.Logging;
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
        _logCommand = new LogCommand(_mockSenderGateway.Object, _mockExpenseApiClient.Object, new Mock<IChatStateService>().Object,
            new Mock<ILogger<LogCommand>>().Object);
    }

    private void MockSender(Chat chat, string expectedReplyText, string? expectedLogMessage = null)
    {
        _mockSenderGateway
            .Setup(x => x.ReplyAsync(chat, expectedReplyText,
                expectedLogMessage ?? "Logged expense.", It.IsAny<LogLevel>(),
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
            .Callback<Expense, CancellationToken>((expense, _) => capturedExpense = expense) // Capture the Expense object
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

    [Theory]
    [InlineData("/log Invalid", "Invalid message format")] // Too few arguments
    [InlineData("log 10", "Invalid message format")] // Too few arguments (no slash)
    [InlineData("log Invalid", "Invalid message format")] // Too few arguments (no slash)
    [InlineData("Invalid", "doesn't start with log command")] // Too few arguments (no slash)
    [InlineData("/log Coffee abc", "Invalid amount format")] // Too few arguments (no slash)
    public async Task HandleLogAsync_InvalidMessageFormat_ThrowsInvalidUserInputException(string messageText, string exceptionMessage)
    {
        // Arrange
        var message = new Message
        {
            Chat = new Chat { Id = 123 },
            Text = messageText
        };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidUserInputException>(() => _logCommand.HandleLogAsync(message, cancellationToken));
        Assert.Contains(exceptionMessage, exception.Message, StringComparison.OrdinalIgnoreCase);

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
    public async Task HandleLogAsync_MessageNotStartingWithLog_ThrowsInvalidUserInputException()
    {
        // Arrange
        var message = new Message { Chat = new Chat { Id = 123 }, Text = "hello world" };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidUserInputException>(() => _logCommand.HandleLogAsync(message, cancellationToken));
        Assert.Contains("doesn't start with log command", exception.Message);
    }
}