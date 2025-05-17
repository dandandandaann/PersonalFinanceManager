using System.Text.Json.Serialization;
using SharedLibrary;

namespace BudgetBotTelegram.AtoTypes;

[JsonSerializable(typeof(LogExpenseResponse))]
[JsonSerializable(typeof(SharedLibrary.UserClasses.UserResponse))]
[JsonSerializable(typeof(SharedLibrary.UserClasses.User))]
[JsonSerializable(typeof(SharedLibrary.UserClasses.UserSignupRequest))]
[JsonSerializable(typeof(SharedLibrary.UserClasses.UserExistsResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(SharedLibrary.Telegram.Update))]
[JsonSerializable(typeof(SharedLibrary.Telegram.Chat))]
[JsonSerializable(typeof(SharedLibrary.Telegram.Message))]
[JsonSerializable(typeof(SharedLibrary.Telegram.MessageEntity))]
[JsonSerializable(typeof(SharedLibrary.Telegram.User))]
[JsonSerializable(typeof(SharedLibrary.Telegram.CallbackQuery))]
public partial class AppTelegramJsonSerializerContext : JsonSerializerContext
{
}