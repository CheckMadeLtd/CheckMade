using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Model.BotPrompts;
using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IOutputDtoToReplyMarkupConverter
{
    Option<IReplyMarkup> GetReplyMarkup(OutputDto output);
}

internal class OutputDtoToReplyMarkupConverter(IUiTranslator translator) : IOutputDtoToReplyMarkupConverter
{
    public Option<IReplyMarkup> GetReplyMarkup(OutputDto output)
    {
        var inlineKeyboardMarkup = output.BotPrompts.Match(
            GetInlineKeyboardMarkup,
            Option<InlineKeyboardMarkup>.None);

        var replyKeyboardMarkup = output.PredefinedChoices.Match(
            GetReplyKeyboardMarkup,
            Option<ReplyKeyboardMarkup>.None);
        
        return 
            inlineKeyboardMarkup.IsSome 
                ? inlineKeyboardMarkup.GetValueOrDefault()
                : replyKeyboardMarkup.IsSome 
                    ? replyKeyboardMarkup.GetValueOrDefault()
                    : Option<IReplyMarkup>.None();
    }

    private Option<InlineKeyboardMarkup> GetInlineKeyboardMarkup(IEnumerable<BotPrompt> prompts)
    {
        const int numberOfColumns = 2;
        
        var inlineKeyboardMatrix = prompts
            .Select((item, index) => new { Index = index, BotPrompt = item })
            .GroupBy(x => x.Index / numberOfColumns)
            .Select(x => 
                x.Select(bp => 
                        InlineKeyboardButton.WithCallbackData(
                            translator.Translate(bp.BotPrompt.Text), 
                            bp.BotPrompt.Id))
                .ToArray())
            .ToArray();
        
        return new InlineKeyboardMarkup(inlineKeyboardMatrix);
    }
    
    private Option<ReplyKeyboardMarkup> GetReplyKeyboardMarkup(IEnumerable<string> choices)
    {
        return Option<ReplyKeyboardMarkup>.None();
    }
}