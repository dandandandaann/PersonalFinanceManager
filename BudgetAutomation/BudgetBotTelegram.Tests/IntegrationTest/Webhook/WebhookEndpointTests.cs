using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Telegram.Bot.Types;
using Xunit;

namespace BudgetBotTelegram.Tests.IntegrationTest.Webhook;

public class WebhookEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private const string WebhookPath = "/webhook";
    private const string ValidTestToken = "WebhookToken"; // Renamed back to a plausible secret

    // Constructor updated to remove ITestOutputHelper
    public WebhookEndpointTests(CustomWebApplicationFactory factory)
    {
        // Ensure the token used here matches the one configured in the Factory
        factory.TestWebhookToken = ValidTestToken;
        _factory = factory;
        _client = factory.CreateClient();
    }

    private StringContent CreateMinimalUpdatePayload()
    {
        var update = new Update { Id = 1 };
        var jsonPayload = JsonSerializer.Serialize(update);
        return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    }

    [Fact]
    public async Task PostWebhook_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var url = $"{WebhookPath}?token={ValidTestToken}";
        var payload = CreateMinimalUpdatePayload();

        _factory.MockUpdateHandler.Reset(); // Reset mock before use
        _factory.MockUpdateHandler
            .Setup(h => h.HandleUpdateAsync(It.IsAny<Update>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.PostAsync(url, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostWebhook_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidToken = "INVALID_TOKEN_XYZ";
        var url = $"{WebhookPath}?token={invalidToken}";
        var payload = CreateMinimalUpdatePayload();

        _factory.MockUpdateHandler.Reset();

        // Act
        var response = await _client.PostAsync(url, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        // Verify the handler was not called since authorization failed
        _factory.MockUpdateHandler.Verify(h => h.HandleUpdateAsync(It.IsAny<Update>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PostWebhook_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var url = WebhookPath; // No token query parameter
        var payload = CreateMinimalUpdatePayload();

        _factory.MockUpdateHandler.Reset();

        // Act
        var response = await _client.PostAsync(url, payload);

        // Assert
        // Assuming the endpoint treats a missing token as unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _factory.MockUpdateHandler.Verify(h => h.HandleUpdateAsync(It.IsAny<Update>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}