using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Telegram.UpdateHandling;

public sealed record UpdateWrapper
{
    internal Update Update { get; }
    internal Message Message { get; }

    private readonly Action<Message> _checkFromIdNotNullOrThrow = static m =>
    {
        if (m.From?.Id == null)
            throw new ArgumentNullException(nameof(m.From), 
                $"A valid message must have a {nameof(m.From)}.{nameof(m.From.Id)}");
    };

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
            _ => throw new InvalidOperationException(
                "This update type should have not been attempted to be wrapped. " +
                "It should have been filtered out when it first came in.")
        } ?? throw new InvalidOperationException("By definition, Message at this point can't be null.");

        // Otherwise the Id of the Bot instead of the actual User is stored (only an issue with CallbackQuery !!)
        if (update.Type == UpdateType.CallbackQuery)
        {
            Message.From!.Id = update.CallbackQuery!.From.Id; // instead of the original CallbackQuery.Message.From.Id
        }
        // We are not doing the equivalent swap for MessageId
        // I.e. it seems to make more sense to not swap CallbackQuery.Message.MessageId for CallbackQuery.Id !!

        _checkFromIdNotNullOrThrow(Message);
    }

    internal UpdateWrapper(Message message)
    {
        Message = message;
        // Needed only by fake messages from TestUtils, which are using this constructor overload as a shortcut.
        Update = new Update { Message = message };
         
        _checkFromIdNotNullOrThrow(Message);
    }
    
    internal UpdateWrapper(Update update, Message message)
    {
        Update = update;
        Message = message;

        _checkFromIdNotNullOrThrow(Message);
    }
}