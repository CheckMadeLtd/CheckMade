using System.ComponentModel;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using General.Utils.Validators;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Bot.Telegram.Conversion;

public interface IOutputToReplyMarkupConverter
{
    Option<ReplyMarkup> GetReplyMarkup(Output output);
}

internal sealed class OutputToReplyMarkupConverter(IUiTranslator translator, IDomainGlossary domainGlossary) 
    : IOutputToReplyMarkupConverter
{
    public Option<ReplyMarkup> GetReplyMarkup(Output output)
    {
        if (!AllEnumsAreDefined(output.ControlPromptsSelection))
            throw new InvalidEnumArgumentException("Some enums are undefined!");
        
        var textCallbackIdPairsForDomainTerms = 
            GetTextIdPairsForDomainTerms(
                output.DomainTermSelection, translator, domainGlossary);

        var inlineKeyboardButtonsForDomainTerms = 
            GenerateInlineKeyboardButtonsForDomainTerms(
                textCallbackIdPairsForDomainTerms);

        var promptsGlossary = new ControlPromptsGlossary();
        
        var textCallbackIdPairsForControlPrompts = 
            GetTextIdPairsForControlPrompts(
                output.ControlPromptsSelection, translator, promptsGlossary);
        
        var inlineKeyboardButtonsForControlPrompts = 
            GenerateInlineKeyboardButtonsForControlPrompts(
                textCallbackIdPairsForControlPrompts);

        var combinedInlineKeyboardMarkup = 
            GenerateInlineKeyboardMarkup(
                inlineKeyboardButtonsForDomainTerms,
                inlineKeyboardButtonsForControlPrompts);
        
        var replyKeyboardMarkup = output.PredefinedChoices.Match(
            GenerateReplyKeyboardMarkup,
            Option<ReplyKeyboardMarkup>.None);

        return combinedInlineKeyboardMarkup.Match(
            static markup => markup,
            () => replyKeyboardMarkup.Match(
                static markup => markup,
                Option<ReplyMarkup>.None));
    }

    private static bool AllEnumsAreDefined(
        Option<ControlPrompts> promptsSelection)
    {
        var allTrue = true;
        
        allTrue &= promptsSelection.Match(
            EnumChecker.IsDefined, 
            static () => true);

        return allTrue;
    }

    private static IReadOnlyCollection<(string text, string id)> GetTextIdPairsForDomainTerms(
        Option<IReadOnlyCollection<DomainTerm>> domainTermSelection,
        IUiTranslator translator,
        IDomainGlossary glossary)
    {
        return domainTermSelection.Match(
            terms =>
            {
                return terms.Select(term => (
                    text: translator.Translate(glossary.GetUi(term)) + 
                          (term.Toggle.IsSome 
                              ? term.Toggle.GetValueOrThrow() 
                                  ? $" {IDomainGlossary.ToggleOnSuffix.GetFormattedEnglish()}" 
                                  : $" {IDomainGlossary.ToggleOffSuffix.GetFormattedEnglish()}" 
                              : string.Empty),
                    id: glossary.GetId(term)
                )).ToList();
            }, 
            static () => []
        ).ToArray();
    }
    
    private static InlineKeyboardButton[][] GenerateInlineKeyboardButtonsForDomainTerms(
        IReadOnlyCollection<(string text, string id)> textCallbackIdPairsForDomainTerms)
    {
        // Note the documented pitfall about medium length instructions problem!
        const int inlineKeyboardNumberOfColumns = 1;

        return GetInlineKeyboardButtonsFromTextIdPairs(
            inlineKeyboardNumberOfColumns,
            textCallbackIdPairsForDomainTerms);
    }
    
    private static IReadOnlyCollection<(string text, string id)> GetTextIdPairsForControlPrompts(
        Option<ControlPrompts> promptSelection,
        IUiTranslator translator,
        ControlPromptsGlossary glossary)
    {
        // For uniformity, convert the combined flagged enum into an array.
        
        var allControlPrompts = 
            Enum.GetValues(typeof(ControlPrompts)).Cast<ControlPrompts>();
        
        var promptSelectionAsCollection = 
            allControlPrompts
                .Where(prompts => 
                    promptSelection.GetValueOrDefault().HasFlag(prompts) && 
                    IsSingleFlag(prompts))
                .ToArray();
        
        return promptSelectionAsCollection.Select(prompt =>
                (text: translator.Translate(glossary.UiByCallbackId[
                        new CallbackId((long)prompt)]),
                    id: new CallbackId((long)prompt).Id))
            .ToArray();
        
        static bool IsSingleFlag(Enum value)
        {
            var buffer = Convert.ToUInt64(value);
            
            return buffer != 0 && (buffer & (buffer - 1)) == 0;
        }
    }

    private static InlineKeyboardButton[][] GenerateInlineKeyboardButtonsForControlPrompts(
        IReadOnlyCollection<(string text, string id)> textCallbackIdPairsForControlPrompts)
    {
        // Note the documented pitfall about medium length instructions problem!
        const int inlineKeyboardNumberOfColumns = 2;

        return GetInlineKeyboardButtonsFromTextIdPairs(
            inlineKeyboardNumberOfColumns,
            textCallbackIdPairsForControlPrompts);
    }
    
    private static InlineKeyboardButton[][] GetInlineKeyboardButtonsFromTextIdPairs(
        int inlineKeyboardNumberOfColumns,
        IReadOnlyCollection<(string text, string id)> textIdPairs)
    {
        return textIdPairs
            .Select(static (pair, index) => new { Index = index, Pair = pair })
            .GroupBy(x => x.Index / inlineKeyboardNumberOfColumns)
            .Select(static x =>
                x.Select(static p =>
                        InlineKeyboardButton.WithCallbackData(
                            p.Pair.text,
                            p.Pair.id))
                    .ToArray())
            .ToArray();
    }
    
    private static Option<InlineKeyboardMarkup> GenerateInlineKeyboardMarkup(
        InlineKeyboardButton[][] domainTermButtons,
        InlineKeyboardButton[][] controlPromptsButtons)
    {
        if (domainTermButtons.Length + controlPromptsButtons.Length == 0)
            return Option<InlineKeyboardMarkup>.None();

        return new InlineKeyboardMarkup(
            domainTermButtons.Concat(
                controlPromptsButtons));
    }
    
    private static Option<ReplyKeyboardMarkup> GenerateReplyKeyboardMarkup(IReadOnlyCollection<string> choices)
    {
        const int replyKeyboardNumberOfColumns = 2;

        var replyKeyboardTable = choices
            .Select(static (item, index) => new { Index = index, Choice = item })
            .GroupBy(static x => x.Index / replyKeyboardNumberOfColumns)
            .Select(static x =>
                x.Select(static c => new KeyboardButton(c.Choice)).ToArray())
            .ToArray();
        
        return new ReplyKeyboardMarkup(replyKeyboardTable)
        {
            IsPersistent = false,
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };
    }
}