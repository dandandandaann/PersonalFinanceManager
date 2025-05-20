using System.Text.Json.Serialization;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace BudgetBotTelegram.AtoTypes;

[JsonSerializable(typeof(LogExpenseResponse))]
[JsonSerializable(typeof(UserSignupResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(UserGetResponse))]
[JsonSerializable(typeof(UserConfiguration))]
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