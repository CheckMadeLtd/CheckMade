using System.ComponentModel;
using CheckMade.Common.Model.Enums.UserInteraction;
using CheckMade.Common.Model.Enums.UserInteraction.Helpers;
using CheckMade.Common.Model.Tlg.Output;
using CheckMade.Common.Utils.Generic;
using CheckMade.Common.Utils.UiTranslation;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Telegram.Function.Services.Conversion;

public interface IOutputToReplyMarkupConverter
{
    Option<IReplyMarkup> GetReplyMarkup(OutputDto output);
}

internal class OutputToReplyMarkupConverter(IUiTranslator translator) : IOutputToReplyMarkupConverter
{
    public Option<IReplyMarkup> GetReplyMarkup(OutputDto output)
    {
        if (!AllEnumsAreDefined(output.DomainCategorySelection, output.ControlPromptsSelection))
            throw new InvalidEnumArgumentException("Some enums are undefined!");
        
        var textCallbackIdPairs = GetTextIdPairsForInlineKeyboardButtons(
            output.DomainCategorySelection,
            output.ControlPromptsSelection,
            translator);
        
        var inlineKeyboardMarkup = 
            GenerateInlineKeyboardMarkup(textCallbackIdPairs.ToList().AsReadOnly());

        var replyKeyboardMarkup = output.PredefinedChoices.Match(
            GenerateReplyKeyboardMarkup,
            Option<ReplyKeyboardMarkup>.None);

        return inlineKeyboardMarkup.Match(
            markup => markup,
            () => replyKeyboardMarkup.Match(
                markup => markup,
                Option<IReplyMarkup>.None));
    }

    private static bool AllEnumsAreDefined(
        Option<IEnumerable<DomainCategory>> categorySelection,
        Option<IEnumerable<ControlPrompts>> promptsSelection)
    {
        var allTrue = true;
        
        allTrue &= categorySelection.Match(
            items => items.All(EnumChecker.IsDefined),
            () => true);
        
        allTrue &= promptsSelection.Match(
            items => items.All(EnumChecker.IsDefined),
            () => true);

        return allTrue;
    }
    
    private static IEnumerable<(string text, string id)> GetTextIdPairsForInlineKeyboardButtons(
        Option<IEnumerable<DomainCategory>> categorySelection,
        Option<IEnumerable<ControlPrompts>> promptSelection,
        IUiTranslator translator)
    {
        var uiStringProvider = new EnumUiStringProvider();

        Func<DomainCategory, string> categoryTranslationGetter =
            category => translator.Translate(uiStringProvider.ByDomainCategoryId[new EnumCallbackId((int)category)]);
        Func<DomainCategory, string> categoryIdGetter = category => new EnumCallbackId((int)category).Id;
        
        Func<ControlPrompts, string> promptTranslationGetter =
            prompt => translator.Translate(uiStringProvider.ByControlPromptId[new EnumCallbackId((long)prompt)]);
        Func<ControlPrompts, string> promptIdGetter = prompt => new EnumCallbackId((long)prompt).Id;

        return CollectTextIdPairs(categorySelection, categoryTranslationGetter, categoryIdGetter)
            .Concat(CollectTextIdPairs(promptSelection, promptTranslationGetter, promptIdGetter));
    }
    
    private static IEnumerable<(string text, string id)> CollectTextIdPairs<TEnum>(
        Option<IEnumerable<TEnum>> selections,
        Func<TEnum, string> translationGetter,
        Func<TEnum, string> idGetter) where TEnum : Enum
    {
        return selections.Match(
            items => items.Select(item =>
                (text: translationGetter(item),
                    id: idGetter(item))),
            Array.Empty<(string text, string id)>);
    }
    
    private static Option<InlineKeyboardMarkup> GenerateInlineKeyboardMarkup(
        IReadOnlyCollection<(string text, string id)> textIdPairs)
    {
        const int inlineKeyboardNumberOfColumns = 2;

        return textIdPairs.Count switch
        {
            0 => Option<InlineKeyboardMarkup>.None(),

            _ => new InlineKeyboardMarkup(textIdPairs
                .Select((pair, index) => new { Index = index, Pair = pair })
                .GroupBy(x => x.Index / inlineKeyboardNumberOfColumns)
                .Select(x =>
                    x.Select(p =>
                            InlineKeyboardButton.WithCallbackData(
                                p.Pair.text,
                                p.Pair.id))
                        .ToArray())
                .ToArray())
        };
    }
    
    private static Option<ReplyKeyboardMarkup> GenerateReplyKeyboardMarkup(IEnumerable<string> choices)
    {
        const int replyKeyboardNumberOfColumns = 3;

        var replyKeyboardTable = choices
            .Select((item, index) => new { Index = index, Choice = item })
            .GroupBy(x => x.Index / replyKeyboardNumberOfColumns)
            .Select(x =>
                x.Select(c => new KeyboardButton(c.Choice)).ToArray())
            .ToArray();
        
        return new ReplyKeyboardMarkup(replyKeyboardTable)
        {
            IsPersistent = false,
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };
    }
}