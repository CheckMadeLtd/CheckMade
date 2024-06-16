using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

public record UpdateWrapper
{
    internal Update Update { get; }
    internal Message Message { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    internal UpdateWrapper(Update update)
    {
        Update = update;
        
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        Message = update.Type switch
        {
            UpdateType.Message => update.Message,
            UpdateType.EditedMessage => update.EditedMessage,
            UpdateType.CallbackQuery => update.CallbackQuery!.Message,
            _ => throw new InvalidOperationException("This update type is not handled as a message (yet).")
        } ?? throw new InvalidOperationException("By definition, Message at this point can't be null.");
    }

    internal UpdateWrapper(Message message)
    {
        Message = message;
        
        // Needed only by fake messages from TestUtils, which are using this constructor overload as a shortcut.
        Update = new Update { Message = message };
    }
    
    internal UpdateWrapper(Update update, Message message)
    {
        Update = update;
        Message = message;
    }
}