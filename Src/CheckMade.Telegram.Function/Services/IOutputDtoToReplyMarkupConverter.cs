using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Telegram.Function.Services;

public interface IOutputDtoToReplyMarkupConverter
{
    Option<IReplyMarkup> GetReplyMarkup(OutputDto output);
}

public class OutputDtoToReplyMarkupConverter : IOutputDtoToReplyMarkupConverter
{
    public Option<IReplyMarkup> GetReplyMarkup(OutputDto output)
    {
        return Option<IReplyMarkup>.None();
    }
}