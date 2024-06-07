using System.ComponentModel;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Tests.Unit.Telegram.Function;

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
            (category: DomainCategory.SanitaryOps_FacilityToilets,
                categoryId: new EnumCallbackId((int)DomainCategory.SanitaryOps_FacilityToilets)),
            (category: DomainCategory.SanitaryOps_FacilityShowers,
                categoryId: new EnumCallbackId((int)DomainCategory.SanitaryOps_FacilityShowers)),
            (category: DomainCategory.SanitaryOps_FacilityStaff,
                categoryId: new EnumCallbackId((int)DomainCategory.SanitaryOps_FacilityStaff)) 
        };
        var fakeOutput = new OutputDto
        {
            ExplicitDestination = basics.fakeDestination,
            DomainCategorySelection = categorySelection.Select(pair => pair.category).ToArray()
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByCategoryId[categorySelection[0].categoryId].GetFormattedEnglish(),
                        categorySelection[0].categoryId.Id),
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByCategoryId[categorySelection[1].categoryId].GetFormattedEnglish(),
                        categorySelection[1].categoryId.Id)
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByCategoryId[categorySelection[2].categoryId].GetFormattedEnglish(),
                        categorySelection[2].categoryId.Id)
                ]
            }));

        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidControlPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var promptSelection = new[]
        {
            (prompt: ControlPrompts.No, promptId: new EnumCallbackId((long)ControlPrompts.No)),
            (prompt: ControlPrompts.Yes, promptId: new EnumCallbackId((long)ControlPrompts.Yes)),
            (prompt: ControlPrompts.Bad, promptId: new EnumCallbackId((long)ControlPrompts.Bad)),
            (prompt: ControlPrompts.Ok, promptId: new EnumCallbackId((long)ControlPrompts.Ok)),
            (prompt: ControlPrompts.Good, promptId: new EnumCallbackId((long)ControlPrompts.Good))
        };
        var fakeOutput = new OutputDto
        {
            ExplicitDestination = basics.fakeDestination,
            ControlPromptsSelection = promptSelection.Select(pair => pair.prompt).ToArray()
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
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsInlineKeyboardCombiningCategoriesAndPrompts_ForOutputWithBoth()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var categorySelection = new[]
        {
            (category: DomainCategory.SanitaryOps_FacilityShowers,
                categoryId: new EnumCallbackId((int)DomainCategory.SanitaryOps_FacilityShowers))
        };
        var promptSelection = new[] 
        {
            (prompt: ControlPrompts.Good, promptId: new EnumCallbackId((long)ControlPrompts.Good))
        };
        var fakeOutput = new OutputDto
        {
            ExplicitDestination = basics.fakeDestination,
            DomainCategorySelection = categorySelection.Select(pair => pair.category).ToArray(),
            ControlPromptsSelection = promptSelection.Select(pair => pair.prompt).ToArray()
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    basics.uiByCategoryId[categorySelection[0].categoryId].GetFormattedEnglish(),
                    categorySelection[0].categoryId.Id),
                InlineKeyboardButton.WithCallbackData(
                    basics.uiByPromptId[promptSelection[0].promptId].GetFormattedEnglish(),
                    promptSelection[0].promptId.Id)
            }));

        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
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
        
        var fakeOutput = new OutputDto
        {
            ExplicitDestination = basics.fakeDestination,
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
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrThrow(), actualReplyMarkup.GetValueOrThrow());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsNone_ForOutputWithoutPromptsOrPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var fakeOutput = new OutputDto { Text = UiNoTranslate("some fake output text") };
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(Option<IReplyMarkup>.None(), actualReplyMarkup);
    }

    [Fact]
    public void GetReplyMarkup_Throws_WhenOutputIncludesInvalidEnum()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var fakeOutput = new OutputDto
        {
            ExplicitDestination = basics.fakeDestination,
            ControlPromptsSelection = new[] { ControlPrompts.Back + 1 }
        };

        var act = () => basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Throws<InvalidEnumArgumentException>(act);
    }
    
    private static (IOutputToReplyMarkupConverter converter, 
        IReadOnlyDictionary<EnumCallbackId, UiString> uiByCategoryId,
        IReadOnlyDictionary<EnumCallbackId, UiString> uiByPromptId,
        TelegramOutputDestination fakeDestination) 
        GetBasicTestingServices(IServiceProvider sp)
    {
        var converterFactory = sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>();
        var converter = converterFactory.Create(new UiTranslator(
            Option<IReadOnlyDictionary<string, string>>.None(),
            sp.GetRequiredService<ILogger<UiTranslator>>()));

        var enumUiStringProvider = new EnumUiStringProvider();
        var uiByCategoryId = enumUiStringProvider.ByDomainCategoryId;
        var uiByPromptId = enumUiStringProvider.ByControlPromptId;
        var fakeDestination = new TelegramOutputDestination(
            TestUtils.SanitaryOpsInspector1, BotType.Operations);
        
        return (converter, uiByCategoryId, uiByPromptId, fakeDestination);
    }
}