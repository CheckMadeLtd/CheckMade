using System.ComponentModel;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;
using static CheckMade.Common.Model.ChatBot.UserInteraction.ControlPrompts;

namespace CheckMade.Tests.Unit.ChatBot.Function;

public class OutputToReplyMarkupConverterTests
{
    private ServiceProvider? _services;

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidDomainCategories()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        List<DomainTerm> domainTermSelection = [ 
            Dt(typeof(CleanlinessIssue)),
            Dt(typeof(TechnicalIssue)),
            Dt(typeof(InventoryIssue))];

        var outputWithDomainTerms = new OutputDto
        {
            DomainTermSelection = domainTermSelection
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary.IdAndUiByTerm[
                                Dt(typeof(CleanlinessIssue))].uiString.GetFormattedEnglish(),
                        basics.domainGlossary.IdAndUiByTerm[
                            Dt(typeof(CleanlinessIssue))].callbackId), 
                        
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary.IdAndUiByTerm[
                            Dt(typeof(TechnicalIssue))].uiString.GetFormattedEnglish(),
                        basics.domainGlossary.IdAndUiByTerm[
                            Dt(typeof(TechnicalIssue))].callbackId), 
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary.IdAndUiByTerm[
                            Dt(typeof(InventoryIssue))].uiString.GetFormattedEnglish(),
                        basics.domainGlossary.IdAndUiByTerm[
                            Dt(typeof(InventoryIssue))].callbackId), 
                ]
            }));

        var actualReplyMarkup = 
            basics.converter.GetReplyMarkup(outputWithDomainTerms);
        
        Assert.Equivalent(
            expectedReplyMarkup.GetValueOrThrow(),
            actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidControlPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var promptSelection = new[]
        {
            (prompt: No, promptId: new CallbackId((long)No)),
            (prompt: Yes, promptId: new CallbackId((long)Yes)),
            (prompt: Bad, promptId: new CallbackId((long)Bad)),
            (prompt: Ok, promptId: new CallbackId((long)Ok)),
            (prompt: Good, promptId: new CallbackId((long)Good))
        };
        var outputWithPrompts = new OutputDto
        {
            ControlPromptsSelection = 
                promptSelection
                    .Select(pair => pair.prompt)
                    .Aggregate((current, next) => current | next)
        };

        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[] 
            { 
                new [] 
                { 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[0].promptId].GetFormattedEnglish(), 
                        promptSelection[0].promptId), 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[1].promptId].GetFormattedEnglish(), 
                        promptSelection[1].promptId) 
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[2].promptId].GetFormattedEnglish(), 
                        promptSelection[2].promptId), 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[3].promptId].GetFormattedEnglish(), 
                        promptSelection[3].promptId) 
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[4].promptId].GetFormattedEnglish(), 
                        promptSelection[4].promptId) 
                ]
            }));
        
        var actualReplyMarkup = 
            basics.converter.GetReplyMarkup(outputWithPrompts);
        
        Assert.Equivalent(
            expectedReplyMarkup.GetValueOrThrow(),
            actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsInlineKeyboardCombiningCategoriesAndPrompts_ForOutputWithBoth()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        List<DomainTerm> domainTermSelection = [
            Dt(Consumables.Item.PaperTowels)];
        
        var promptSelection = new[] 
        {
            (prompt: Good, promptId: new CallbackId((long)Good))
        };
        
        var outputWithBoth = new OutputDto
        {
            DomainTermSelection = domainTermSelection,
            
            ControlPromptsSelection = 
                promptSelection
                    .Select(pair => pair.prompt)
                    .Aggregate((current, next) => current | next)
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    basics.domainGlossary.IdAndUiByTerm[
                        Dt(Consumables.Item.PaperTowels)].uiString.GetFormattedEnglish(),
                    basics.domainGlossary.IdAndUiByTerm[
                        Dt(Consumables.Item.PaperTowels)].callbackId), 
                
                InlineKeyboardButton.WithCallbackData(
                    basics.uiByPromptId[promptSelection[0].promptId].GetFormattedEnglish(),
                    promptSelection[0].promptId)
            }));

        var actualReplyMarkup = 
            basics.converter.GetReplyMarkup(outputWithBoth);
        
        Assert.Equivalent(
            expectedReplyMarkup.GetValueOrThrow(),
            actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedReplyKeyboard_ForValidPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string choice1 = "c1";
        const string choice2 = "c2";
        const string choice3 = "c3";
        const string choice4 = "c4";
        const string choice5 = "c5";
        
        var outputWithChoices = new OutputDto
        {
            PredefinedChoices = new[] { choice1, choice2, choice3, choice4, choice5 }   
        };
        
        // Assumes replyKeyboardNumberOfColumns = 3
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(new ReplyKeyboardMarkup(new[]
        {
            new[] 
                { new KeyboardButton(choice1), new KeyboardButton(choice2), new KeyboardButton(choice3) },
                [new KeyboardButton(choice4), new KeyboardButton(choice5)]
        })
        {
            IsPersistent = false,
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        });
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(outputWithChoices);
        
        Assert.Equivalent(
            expectedReplyMarkup.GetValueOrThrow(),
            actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsNone_ForOutputWithoutPromptsOrPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var outputWithout = new OutputDto();
        
        var actualReplyMarkup = 
            basics.converter.GetReplyMarkup(outputWithout);
        
        Assert.Equivalent(
            Option<IReplyMarkup>.None(),
            actualReplyMarkup);
    }

    [Fact]
    public void GetReplyMarkup_Throws_WhenOutputIncludesInvalidEnum()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var outputWithInvalid = new OutputDto
        {
            ControlPromptsSelection = Back + 1
        };

        var act = () => 
            basics.converter.GetReplyMarkup(outputWithInvalid);
        
        Assert.Throws<InvalidEnumArgumentException>(act);
    }
    
    private static (IOutputToReplyMarkupConverter converter, 
        IReadOnlyDictionary<CallbackId, UiString> uiByPromptId,
        DomainGlossary domainGlossary) 
        GetBasicTestingServices(IServiceProvider sp)
    {
        var converterFactory = sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>();
        var converter = converterFactory.Create(new UiTranslator(
            Option<IReadOnlyDictionary<string, string>>.None(),
            sp.GetRequiredService<ILogger<UiTranslator>>()));

        var controlPromptsGlossary = new ControlPromptsGlossary();
        var uiByPromptId = controlPromptsGlossary.UiByCallbackId;
        
        return (converter, uiByPromptId, new DomainGlossary());
    }
}