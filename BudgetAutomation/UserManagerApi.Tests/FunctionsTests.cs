// This file was mostly AI generated.

using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Moq;
using SharedLibrary.Dto;
using SharedLibrary.Model;
using Shouldly;
using Xunit;

namespace UserManagerApi.Tests;

public class FunctionsTests
{
    // --- Mocks ---
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly Mock<ILambdaContext> _mockLambdaContext;

    // --- Mock for the nested DynamoDB call result ---
    private readonly Mock<AsyncSearch<User>> _mockSearch; // <-- Make this a field

    // --- Class Under Test ---
    private readonly Functions _functions;

    // --- Test Data ---
    private const long DefaultTelegramId = 123456789;
    private const string DefaultUsername = "testuser";
    private const string DefaultEmail = "test@email.com";
    private const string ExistingUserId = "existing-user-guid";
    // We won't predict the new Guid, just check it's generated

    public FunctionsTests()
    {
        _mockDbContext = new Mock<IDynamoDBContext>();
        _mockLambdaContext = new Mock<ILambdaContext>();
        Mock<ILambdaLogger> mockLambdaLogger = new();
        _mockSearch = new Mock<AsyncSearch<User>>(); // <-- Initialize the field here

        // Setup mock context and logger
        _mockLambdaContext.Setup(c => c.Logger).Returns(mockLambdaLogger.Object);

        // Setup FromQueryAsync to return the mock search object's instance
        _mockDbContext.Setup(db => db.FromQueryAsync<User>(It.IsAny<QueryOperationConfig>()))
            .Returns(_mockSearch.Object); // Use the field's Object property

        // Set up a *default* behavior for GetNextSetAsync.
        // Tests can override this specific setup if they need a different result.
        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>()); // Default to not found

        // Instantiate the class under test
        // TODO: uncomment this next line and fix the unit tests according to last changes to the UserManagerApi project
        // _functions = new Functions(_mockDbContext.Object);
        _functions = new Functions(null);
    }

    // Helper to deserialize response body
    private T DeserializeBody<T>(APIGatewayHttpApiV2ProxyResponse response)
    {
        response.Body.ShouldNotBeNullOrEmpty($"Response body was null or empty. Status code: {response.StatusCode}");
        try
        {
            return JsonSerializer.Deserialize<T>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to deserialize body: {response.Body}. Error: {ex.Message}", ex);
        }
    }

    // --- SignupUserAsync Tests ---

    [Fact]
    public async Task SignupUserAsync_WhenUserExists_ShouldReturnOkAndExistingUserId()
    {
        // Arrange
        var request = new UserSignupRequest( DefaultTelegramId, DefaultEmail, DefaultUsername );
        var existingUser = new User { UserId = ExistingUserId, TelegramId = DefaultTelegramId, Username = "oldUsername" };
        var usersFound = new List<User> { existingUser };

        // --- Configure the mock search for this specific test ---
        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>())) // <-- Directly use the _mockSearch field
            .ReturnsAsync(usersFound);

        // Act
        var response = await _functions.SignupUserAsync(request, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(200); // OK

        var responseBody = DeserializeBody<UserGetResponse>(response);
        responseBody.Success.ShouldBeFalse();
        responseBody.UserId.ShouldBe(ExistingUserId);

        _mockDbContext.Verify(db => db.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        // Verify query was called (implicitly via the GetNextSetAsync setup)
        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once); // Explicit verification is good

    }

    [Fact]
    public async Task SignupUserAsync_WhenUserDoesNotExist_ShouldCreateUserAndReturnCreated()
    {
        // Arrange
        var request = new UserSignupRequest( DefaultTelegramId, DefaultEmail, DefaultUsername );

        // --- Configure the mock search for this specific test ---
        // The default setup in the constructor already returns an empty list,
        // but we can be explicit if preferred.
        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>())) // <-- Directly use the _mockSearch field
            .ReturnsAsync(new List<User>());

        _mockDbContext.Setup(db => db.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _functions.SignupUserAsync(request, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(201); // Created

        response.Headers.ShouldContainKey("Location");
        response.Headers["Location"].ShouldStartWith("/user/");

        var responseBody = DeserializeBody<UserSignupResponse>(response);
        responseBody.Success.ShouldBeTrue();
        responseBody.User.ShouldNotBeNull();
        responseBody.User.TelegramId.ShouldBe(DefaultTelegramId);
        responseBody.User.Username.ShouldBe(DefaultUsername);
        responseBody.User.UserId.ShouldNotBeNullOrWhiteSpace();
        responseBody.User.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));

        _mockDbContext.Verify(db => db.SaveAsync(It.Is<User>(u => u.TelegramId == DefaultTelegramId && u.Username == DefaultUsername), It.IsAny<CancellationToken>()), Times.Once);
        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task SignupUserAsync_WhenDbQueryThrows_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new UserSignupRequest( DefaultTelegramId, DefaultEmail );
        var dbException = new InvalidOperationException("DynamoDB query failed"); // Use a specific exception type

        // --- Configure the mock search to throw ---
        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>())) // <-- Directly use the _mockSearch field
            .ThrowsAsync(dbException);

        // Act
        var response = await _functions.SignupUserAsync(request, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(500);
        response.Body.ShouldContain("An error occurred while retrieving the user.");

        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task SignupUserAsync_WhenDbSaveThrows_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new UserSignupRequest(DefaultTelegramId, DefaultEmail, DefaultUsername );
        var dbException = new InvalidOperationException("DynamoDB save failed"); // Use a specific exception type

        // User not found initially
        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>())) // <-- Directly use the _mockSearch field
            .ReturnsAsync(new List<User>());

        // Configure SaveAsync to throw
        _mockDbContext.Setup(db => db.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act
        var response = await _functions.SignupUserAsync(request, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(500);
        response.Body.ShouldContain("An error occurred while retrieving the user.");

        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockDbContext.Verify(db => db.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);

    }

    // --- GetUserByTelegramIdAsync Tests ---

    [Fact]
    public async Task GetUserByTelegramIdAsync_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = "not-a-number";

        // Act
        var response = await _functions.GetUserByTelegramIdAsync(invalidId, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(400);
        response.Body.ShouldContain("Invalid or missing TelegramId in path.");

        // Verify No DB call attempted
        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByTelegramIdAsync_WhenUserFound_ShouldReturnOkWithUser()
    {
        // Arrange
        var telegramIdString = DefaultTelegramId.ToString();
        var existingUser = new User { UserId = ExistingUserId, TelegramId = DefaultTelegramId, Username = DefaultUsername };
        var usersFound = new List<User> { existingUser };

        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>())) // <-- Directly use the _mockSearch field
            .ReturnsAsync(usersFound);

        // Act
        var response = await _functions.GetUserByTelegramIdAsync(telegramIdString, _mockLambdaContext.Object);


        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(200); // OK

        var responseBody = DeserializeBody<UserGetResponse>(response);
        responseBody.ShouldBeOfType<UserGetResponse>();
        responseBody.Success.ShouldBeTrue();
        responseBody.UserId.ShouldBe(ExistingUserId);

        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task GetUserByTelegramIdAsync_WhenUserNotFound_ShouldReturnOkWithNullUser()
    {
        // Arrange
        var telegramIdString = DefaultTelegramId.ToString();

        // Default setup returns empty list, no need to re-configure _mockSearch here
        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>()); // Explicit is also fine

        // Act
        var response = await _functions.GetUserByTelegramIdAsync(telegramIdString, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(200); // OK

        var responseBody = DeserializeBody<UserGetResponse>(response);
        responseBody.ShouldBeOfType<UserGetResponse>();
        responseBody.Success.ShouldBeFalse();
        responseBody.UserId.ShouldNotBe(ExistingUserId);

        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task GetUserByTelegramIdAsync_WhenDbQueryThrows_ShouldReturnInternalServerError()
    {
        // Arrange
        var telegramIdString = DefaultTelegramId.ToString();
        var dbException = new InvalidOperationException("DynamoDB query failed"); // Use a specific exception type

        _mockSearch.Setup(s => s.GetNextSetAsync(It.IsAny<CancellationToken>())) // <-- Directly use the _mockSearch field
            .ThrowsAsync(dbException);

        // Act
        var response = await _functions.GetUserByTelegramIdAsync(telegramIdString, _mockLambdaContext.Object);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(500);
        response.Body.ShouldContain("An error occurred while retrieving the user.");

        _mockSearch.Verify(s => s.GetNextSetAsync(It.IsAny<CancellationToken>()), Times.Once);

    }
}