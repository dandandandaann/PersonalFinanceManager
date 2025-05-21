using System.Text.Json.Serialization;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace BudgetAutomation.Engine.AtoTypes;

[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(LogExpenseResponse))]
[JsonSerializable(typeof(UserConfigurationDto))]
[JsonSerializable(typeof(UserConfigurationUpdateRequest))]
[JsonSerializable(typeof(UserConfigurationUpdateResponse))]
[JsonSerializable(typeof(UserGetResponse))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(UserSignupResponse))]
[JsonSerializable(typeof(UserUpdateResponse))]
[JsonSerializable(typeof(UserUpsertResponse))]

[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserConfiguration))]
public partial class AppJsonSerializerContext : JsonSerializerContext;

[JsonSerializable(typeof(SharedLibrary.Telegram.Update))]
[JsonSerializable(typeof(SharedLibrary.Telegram.Chat))]
[JsonSerializable(typeof(SharedLibrary.Telegram.Message))]
[JsonSerializable(typeof(SharedLibrary.Telegram.MessageEntity))]
[JsonSerializable(typeof(SharedLibrary.Telegram.User))]
[JsonSerializable(typeof(SharedLibrary.Telegram.CallbackQuery))]
public partial class AppTelegramJsonSerializerContext : JsonSerializerContext;