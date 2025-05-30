using Riok.Mapperly.Abstractions;

// Using aliases for better readability
using SharedLibraryBaseReplyMarkup = SharedLibrary.Telegram.Types.ReplyMarkups.ReplyMarkup;
using BotBaseReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyMarkup;

// --- SHARED LIBRARY CONCRETE TYPES ---
using SharedLibraryInlineKeyboardMarkup = SharedLibrary.Telegram.Types.ReplyMarkups.InlineKeyboardMarkup;
using SharedLibraryReplyKeyboardMarkup = SharedLibrary.Telegram.Types.ReplyMarkups.ReplyKeyboardMarkup;
using SharedLibraryReplyKeyboardRemove = SharedLibrary.Telegram.Types.ReplyMarkups.ReplyKeyboardRemove;
using SharedLibraryForceReplyMarkup = SharedLibrary.Telegram.Types.ReplyMarkups.ForceReplyMarkup;

// --- TELEGRAM.BOT CONCRETE TYPES ---
using BotInlineKeyboardMarkup = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup;
using BotReplyKeyboardMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup;
using BotReplyKeyboardRemove = Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove;
using BotForceReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ForceReplyMarkup;

namespace BudgetAutomation.Engine.Mapper;

[Mapper]
public partial class MessageMapper
{
    [MapDerivedType<SharedLibraryInlineKeyboardMarkup, BotInlineKeyboardMarkup>]
    [MapDerivedType<SharedLibraryReplyKeyboardMarkup, BotReplyKeyboardMarkup>]
    [MapDerivedType<SharedLibraryReplyKeyboardRemove, BotReplyKeyboardRemove>]
    [MapDerivedType<SharedLibraryForceReplyMarkup, BotForceReplyMarkup>]
    public partial BotBaseReplyMarkup MapReplyMarkup(SharedLibraryBaseReplyMarkup source);
}