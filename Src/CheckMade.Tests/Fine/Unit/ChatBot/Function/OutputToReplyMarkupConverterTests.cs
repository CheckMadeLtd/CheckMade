using System.ComponentModel;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.Common.Model.ChatBot.Output;
using static CheckMade.Common.Model.Core.DomainCategories;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;
using static CheckMade.Common.Model.ChatBot.UserInteraction.ControlPrompts;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Function;

public class OutputToReplyMarkupConverterTests
{
    private ServiceProvider? _services;

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidDomainCategories()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var categorySelection = new[] 
        {
            (int)SanitaryOpsIssue.Cleanliness,
            (int)SanitaryOpsIssue.Technical,
            (int)SanitaryOpsIssue.Consumable
        };

        var categoryUiStringByCallbackId = categorySelection.ToDictionary(
            cat => DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[cat].callbackId,
            cat => DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[cat].uiString);
        
        var outputWithDomainCategories = new OutputDto
        {
            DomainCategorySelection = categoryUiStringByCallbackId
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                                (int)SanitaryOpsIssue.Cleanliness].uiString.GetFormattedEnglish(),
                        DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                            (int)SanitaryOpsIssue.Cleanliness].callbackId), 
                        
                    InlineKeyboardButton.WithCallbackData(
                        DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                                (int)SanitaryOpsIssue.Technical].uiString.GetFormattedEnglish(),
                        DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                            (int)SanitaryOpsIssue.Technical].callbackId), 
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                                (int)SanitaryOpsIssue.Consumable].uiString.GetFormattedEnglish(),
                        DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                            (int)SanitaryOpsIssue.Consumable].callbackId), 
                ]
            }));

        var actualReplyMarkup = basics.converter.GetReplyMarkup(outputWithDomainCategories);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidControlPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var promptSelection = new[]
        {
            (prompt: No, promptId: new ControlPromptsCallbackId((long)No)),
            (prompt: Yes, promptId: new ControlPromptsCallbackId((long)Yes)),
            (prompt: Bad, promptId: new ControlPromptsCallbackId((long)Bad)),
            (prompt: Ok, promptId: new ControlPromptsCallbackId((long)Ok)),
            (prompt: Good, promptId: new ControlPromptsCallbackId((long)Good))
        };
        var outputWithPrompts = new OutputDto
        {
            ControlPromptsSelection = promptSelection.Select(pair => pair.prompt)
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
                        promptSelection[0].promptId.Id), 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[1].promptId].GetFormattedEnglish(), 
                        promptSelection[1].promptId.Id) 
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[2].promptId].GetFormattedEnglish(), 
                        promptSelection[2].promptId.Id), 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[3].promptId].GetFormattedEnglish(), 
                        promptSelection[3].promptId.Id) 
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[4].promptId].GetFormattedEnglish(), 
                        promptSelection[4].promptId.Id) 
                ]
            }));
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(outputWithPrompts);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsInlineKeyboardCombiningCategoriesAndPrompts_ForOutputWithBoth()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var categorySelection = new[]
        {
            (int)SanitaryOpsIssue.Cleanliness
        };
        
        var categoryUiStringByCallbackId = categorySelection.ToDictionary(
            cat => DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[cat].callbackId,
            cat => DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[cat].uiString);
        
        var promptSelection = new[] 
        {
            (prompt: Good, promptId: new ControlPromptsCallbackId((long)Good))
        };
        
        var outputWithBoth = new OutputDto
        {
            DomainCategorySelection = categoryUiStringByCallbackId,
            ControlPromptsSelection = promptSelection.Select(pair => pair.prompt)
                .Aggregate((current, next) => current | next)
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                            (int)SanitaryOpsIssue.Cleanliness].uiString.GetFormattedEnglish(),
                    DomainCategoryMap.CallbackIdAndUiStringByDomainCategory[
                        (int)SanitaryOpsIssue.Cleanliness].callbackId), 
                
                InlineKeyboardButton.WithCallbackData(
                    basics.uiByPromptId[promptSelection[0].promptId].GetFormattedEnglish(),
                    promptSelection[0].promptId.Id)
            }));

        var actualReplyMarkup = basics.converter.GetReplyMarkup(outputWithBoth);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
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
                [ new KeyboardButton(choice4), new KeyboardButton(choice5) ]
        })
        {
            IsPersistent = false,
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        });
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(outputWithChoices);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsNone_ForOutputWithoutPromptsOrPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var outputWithout = new OutputDto();
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(outputWithout);
        
        Assert.Equivalent(Option<IReplyMarkup>.None(), actualReplyMarkup);
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

        var act = () => basics.converter.GetReplyMarkup(outputWithInvalid);
        
        Assert.Throws<InvalidEnumArgumentException>(act);
    }
    
    private static (IOutputToReplyMarkupConverter converter, 
        IReadOnlyDictionary<ControlPromptsCallbackId, UiString> uiByPromptId) 
        GetBasicTestingServices(IServiceProvider sp)
    {
        var converterFactory = sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>();
        var converter = converterFactory.Create(new UiTranslator(
            Option<IReadOnlyDictionary<string, string>>.None(),
            sp.GetRequiredService<ILogger<UiTranslator>>()));

        var enumUiStringProvider = new ControlPromptsUiStringProvider();
        var uiByPromptId = enumUiStringProvider.ByControlPromptCallbackId;
        
        return (converter, uiByPromptId);
    }
}