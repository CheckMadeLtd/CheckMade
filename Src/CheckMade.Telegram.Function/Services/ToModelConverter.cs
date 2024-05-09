using CheckMade.Telegram.Model;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverter
{
    InputTextMessage ConvertMessage(Message telegramInputMessage);
}

internal class ToModelConverter : IToModelConverter
{
    public InputTextMessage ConvertMessage(Message telegramInputMessage)
    {
        var userId = telegramInputMessage.From?.Id 
                     ?? throw new ArgumentNullException(nameof(telegramInputMessage),
                         "From.Id in the input message must not be null");
        
        var messageText = string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? throw new ArgumentNullException(nameof(telegramInputMessage),
                "Text in the telegram input message must not be empty")
            : telegramInputMessage.Text;
        
        return new InputTextMessage(
            userId,
            new MessageDetails(
                messageText,
                telegramInputMessage.Date));
    }
}
