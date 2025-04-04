using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace BudgetBotTelegram;

// [JsonSerializable(typeof(BotConfiguration))]

// Apply source generation options - SnakeCaseLower is common for Telegram
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    // WriteIndented = true, // Optional: for debugging, disable for production
    // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Optional
    GenerationMode = JsonSourceGenerationMode.Metadata // Use Metadata for AOT
)]

// --- Add ALL types you expect to serialize/deserialize ---

[JsonSerializable(typeof(Update))]

// --- Common nested types ---
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(CallbackQuery))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Chat))]
[JsonSerializable(typeof(WebhookInfo))]
[JsonSerializable(typeof(BotCommand))]
// [JsonSerializable(typeof(InlineQuery))]
// [JsonSerializable(typeof(ChosenInlineResult))]
[JsonSerializable(typeof(MessageEntity))] // Often needed with Message
[JsonSerializable(typeof(IEnumerable<MessageEntity>))] // Collections too

// --- *** MessageOrigin and its derived types *** ---
[JsonSerializable(typeof(MessageOrigin))] // Base abstract class
[JsonDerivedType(typeof(MessageOriginUser), typeDiscriminator: "user")]
[JsonDerivedType(typeof(MessageOriginHiddenUser), typeDiscriminator: "hidden_user")]
[JsonDerivedType(typeof(MessageOriginChat), typeDiscriminator: "chat")]
[JsonDerivedType(typeof(MessageOriginChannel), typeDiscriminator: "channel")]

// Also add JsonSerializable for the derived types themselves
[JsonSerializable(typeof(MessageOriginUser))]
[JsonSerializable(typeof(MessageOriginHiddenUser))]
[JsonSerializable(typeof(MessageOriginChat))]
[JsonSerializable(typeof(MessageOriginChannel))]
// --- *** End of MessageOrigin additions *** ---


// --- Potentially add specific reply markup types if you deserialize them ---
// [JsonSerializable(typeof(InlineKeyboardMarkup))]
// [JsonSerializable(typeof(InlineKeyboardButton))]

// --- Primitive types often involved ---
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(IEnumerable<Update>))]


// --- Your custom types ---
// [JsonSerializable(typeof(MyCustomRequest))]
// [JsonSerializable(typeof(MyCustomResponse))]

// Define the partial context class
public partial class AppJsonSerializerContext : JsonSerializerContext
{
    // The generator will implement this partial class
}