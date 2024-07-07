using System.ComponentModel;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.Generic;
using CheckMade.Common.Utils.UiTranslation;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.ChatBot.Function.Services.Conversion;

public interface IOutputToReplyMarkupConverter
{
    Option<IReplyMarkup> GetReplyMarkup(OutputDto output);
}

internal class OutputToReplyMarkupConverter(IUiTranslator translator) : IOutputToReplyMarkupConverter
{
    public Option<IReplyMarkup> GetReplyMarkup(OutputDto output)
    {
        if (!AllEnumsAreDefined(output.ControlPromptsSelection))
            throw new InvalidEnumArgumentException("Some enums are undefined!");
        
        var textCallbackIdPairs = GetTextIdPairsForInlineKeyboardButtons(
            output.DomainTermSelection,
            output.ControlPromptsSelection,
            translator);
        
        var inlineKeyboardMarkup = 
            GenerateInlineKeyboardMarkup(textCallbackIdPairs.ToImmutableReadOnlyCollection());

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
        Option<ControlPrompts> promptsSelection)
    {
        var allTrue = true;
        
        allTrue &= promptsSelection.Match(
            EnumChecker.IsDefined,
            () => true);

        return allTrue;
    }
    
    private static IReadOnlyCollection<(string text, string id)> GetTextIdPairsForInlineKeyboardButtons(
        Option<IReadOnlyCollection<DomainTerm>> domainTermSelection,
        Option<ControlPrompts> promptSelection,
        IUiTranslator translator)
    {
        var promptsGlossary = new ControlPromptsGlossary();
        var domainGlossary = new DomainGlossary();

        List<(string text, string id)> allTextIdPairs = [];

        allTextIdPairs.AddRange(domainTermSelection.Match(
            terms =>
            {
                return terms.Select(term => (
                    text: translator.Translate(domainGlossary.IdAndUiByTerm[term].uiString),
                    id: domainGlossary.IdAndUiByTerm[term].callbackId.Id
                )).ToList();
            },
            () => []
        ));
        
        // For uniformity, convert the combined flagged enum into an array.
        var allControlPrompts = Enum.GetValues(typeof(ControlPrompts)).Cast<ControlPrompts>();
        var promptSelectionAsCollection = allControlPrompts
            .Where(prompts => 
                promptSelection.GetValueOrDefault().HasFlag(prompts) &&
                 IsSingleFlag(prompts))
            .ToImmutableReadOnlyCollection();
        
        allTextIdPairs.AddRange(promptSelectionAsCollection.Select(prompt =>
            (text: translator.Translate(promptsGlossary.UiByCallbackId[
                new CallbackId((long)prompt)]),
                id: new CallbackId((long)prompt).Id)));

        return allTextIdPairs.ToImmutableReadOnlyCollection();
        
        static bool IsSingleFlag(Enum value)
        {
            var buffer = Convert.ToUInt64(value);
            
            return buffer != 0 && (buffer & (buffer - 1)) == 0;
        }
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
    
    private static Option<ReplyKeyboardMarkup> GenerateReplyKeyboardMarkup(IReadOnlyCollection<string> choices)
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