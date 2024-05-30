using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;
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
            (cat: DomainCategory.SanitaryOpsFacilityToilets,
                catId: new EnumCallbackId((int)DomainCategory.SanitaryOpsFacilityToilets)),
            (cat: DomainCategory.SanitaryOpsFacilityShowers,
                catId: new EnumCallbackId((int)DomainCategory.SanitaryOpsFacilityShowers)),
            (cat: DomainCategory.SanitaryOpsFacilityStaff,
                catId: new EnumCallbackId((int)DomainCategory.SanitaryOpsFacilityStaff)) 
        };
        var fakeOutput = OutputDto.Create(categorySelection.Select(pair => pair.cat).ToArray());
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByCategoryId[categorySelection[0].catId].GetFormattedEnglish(),
                        categorySelection[0].catId.Id),
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByCategoryId[categorySelection[1].catId].GetFormattedEnglish(),
                        categorySelection[1].catId.Id),
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByCategoryId[categorySelection[2].catId].GetFormattedEnglish(),
                        categorySelection[2].catId.Id)
                ]
            }));

        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrDefault(), actualReplyMarkup.GetValueOrDefault());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidControlPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var promptSelection = new[]
        {
            (prompt: ControlPrompts.No, promptId: new EnumCallbackId((int)ControlPrompts.No)),
            (prompt: ControlPrompts.Yes, promptId: new EnumCallbackId((int)ControlPrompts.Yes)),
            (prompt: ControlPrompts.Bad, promptId: new EnumCallbackId((int)ControlPrompts.Bad)),
            (prompt: ControlPrompts.Ok, promptId: new EnumCallbackId((int)ControlPrompts.Ok)),
            (prompt: ControlPrompts.Good, promptId: new EnumCallbackId((int)ControlPrompts.Good))
        };
        var fakeOutput = OutputDto.Create(promptSelection.Select(pair => pair.prompt).ToArray());

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
                        promptSelection[1].promptId.Id), 
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[2].promptId].GetFormattedEnglish(), 
                        promptSelection[2].promptId.Id), 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[3].promptId].GetFormattedEnglish(), 
                        promptSelection[3].promptId.Id), 
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[4].promptId].GetFormattedEnglish(), 
                        promptSelection[4].promptId.Id), 
                ]
            }));
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrDefault(), actualReplyMarkup.GetValueOrDefault());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsInlineKeyboardCombiningCategoriesAndPrompts_ForOutputWithBoth()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        // var categorySelection = 
        var promptSelection = new[]
        {
            (prompt: ControlPrompts.Good, promptId: new EnumCallbackId((int)ControlPrompts.Good))
        };
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
        var fakeOutput = OutputDto.Create(new[] { choice1, choice2, choice3, choice4, choice5 });
        
        // Assumes replyKeyboardNumberOfColumns = 3
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(new ReplyKeyboardMarkup(new[]
        {
            new[] 
                { new KeyboardButton(choice1), new KeyboardButton(choice2), new KeyboardButton(choice3) },
                [ new KeyboardButton(choice4), new KeyboardButton(choice5) ]
        }));
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrDefault(), actualReplyMarkup.GetValueOrDefault());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsOnlyReplyKeyboard_WhenOutputMistakenlyProvidesBothPromptsAndPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var promptSelection = new[]
        {
            (prompt: ControlPrompts.Good, promptId: new EnumCallbackId((int)ControlPrompts.Good))
        };
        const string choice1 = "c1";
        
        // var fakeOutput = new OutputDto()
    }
    
    [Fact]
    public void GetReplyMarkup_ReturnsNone_ForOutputWithoutPromptsOrPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var fakeOutput = OutputDto.CreateEmpty();
        
        var actualReplyMarkup = basics.converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(Option<IReplyMarkup>.None(), actualReplyMarkup);
    }

    private static (IOutputToReplyMarkupConverter converter, 
        IReadOnlyDictionary<EnumCallbackId, UiString> uiByCategoryId,
        IReadOnlyDictionary<EnumCallbackId, UiString> uiByPromptId) 
        GetBasicTestingServices(IServiceProvider sp)
    {
        var converterFactory = sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>();
        var converter = converterFactory.Create(new UiTranslator(
            Option<IReadOnlyDictionary<string, string>>.None(),
            sp.GetRequiredService<ILogger<UiTranslator>>()));

        var enumUiStringProvider = new EnumUiStringProvider();
        var uiByCategoryId = enumUiStringProvider.ByDomainCategoryId;
        var uiByPromptId = enumUiStringProvider.ByControlPromptId;
        
        return (converter, uiByCategoryId, uiByPromptId);
    }
}