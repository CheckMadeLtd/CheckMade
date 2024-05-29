using CheckMade.Common.Model.Enums;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IOutputToReplyMarkupConverter
{
    Option<IReplyMarkup> GetReplyMarkup(OutputDto output);
}

internal class OutputToReplyMarkupConverter(IUiTranslator translator) : IOutputToReplyMarkupConverter
{
    public Option<IReplyMarkup> GetReplyMarkup(OutputDto output)
    {
        var inlineKeyboardMarkup = output.ControlPromptsSelection.Match(
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

    private Option<InlineKeyboardMarkup> GetInlineKeyboardMarkup(IEnumerable<ControlPrompts> prompts)
    {
        const int inlineKeyboardNumberOfColumns = 2;
        var definition = new EnumUiStringProvider();
        
        var inlineKeyboardTable = prompts
            .Select((controlPrompt, index) => 
                new
                {
                    Index = index, 
                    BotPrompt = controlPrompt, 
                    PromptId = new EnumCallbackId((int)controlPrompt)
                })
            .GroupBy(x => x.Index / inlineKeyboardNumberOfColumns)
            .Select(x => 
                x.Select(bp => 
                        InlineKeyboardButton.WithCallbackData(
                            translator.Translate(definition.ByControlPromptId[bp.PromptId]), 
                            bp.PromptId.Id))
                    .ToArray())
            .ToArray();
        
        return new InlineKeyboardMarkup(inlineKeyboardTable);
    }
    
    private Option<ReplyKeyboardMarkup> GetReplyKeyboardMarkup(IEnumerable<string> choices)
    {
        const int replyKeyboardNumberOfColumns = 3;

        var replyKeyboardTable = choices
            .Select((item, index) => new { Index = index, Choice = item })
            .GroupBy(x => x.Index / replyKeyboardNumberOfColumns)
            .Select(x =>
                x.Select(c => new KeyboardButton(c.Choice)).ToArray())
            .ToArray();
        
        return new ReplyKeyboardMarkup(replyKeyboardTable);
    }
}