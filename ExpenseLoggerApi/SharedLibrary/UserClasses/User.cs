using Amazon.DynamoDBv2.DataModel;

namespace SharedLibrary.UserClasses;

[DynamoDBTable("user")]
public class User
{
    private readonly string _userId = string.Empty;

    // Partition Key
    [DynamoDBHashKey("pk")]
    public string Pk { get; set; } = null!;

    // Sort Key
    [DynamoDBRangeKey("sk")]
    public string Sk { get; set; } = null!;

    [DynamoDBProperty("userId")]
    public string UserId
    {
        get => _userId;
        init
        {
            _userId = value;
            Pk = _userId;
            Sk = _userId;
        }
    }

    [DynamoDBProperty("email")]
    public string Email { get; set; } = null!;

    [DynamoDBProperty("username")]
    public string? Username { get; set; }

    [DynamoDBProperty("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [DynamoDBGlobalSecondaryIndexHashKey("TelegramId-index")]
    [DynamoDBProperty("telegramId")]
    public long TelegramId { get; set; }

    [DynamoDBProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    // Parameterless constructor required by DynamoDBContext
    public User() { }

    public User(string userId, string email = "", string passwordHash = "", long? telegramId = null)
    {
        UserId = userId;
        Email = email;
        PasswordHash = passwordHash;
        TelegramId = telegramId ?? 0;
        CreatedAt = DateTime.UtcNow;
    }
}