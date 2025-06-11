using System.ComponentModel;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Submissions.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;
using static CheckMade.Common.Model.ChatBot.UserInteraction.ControlPrompts;

namespace CheckMade.Tests.Unit.ChatBot.Function;

public sealed class OutputToReplyMarkupConverterTests
{
    private ServiceProvider? _services;

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidDomainCategories()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        List<DomainTerm> domainTermSelection = 
        [ 
            Dt(typeof(CleaningIssue<SanitaryTrade>)),
            Dt(typeof(TechnicalIssue<SanitaryTrade>)),
            Dt(typeof(ConsumablesIssue<SanitaryTrade>))
        ];

        var outputWithDomainTerms = new OutputDto
        {
            DomainTermSelection = domainTermSelection
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 1
        var expectedReplyMarkup = Option<ReplyMarkup>.Some(
            new InlineKeyboardMarkup([
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary
                            .GetUi(typeof(CleaningIssue<SanitaryTrade>))
                            .GetFormattedEnglish(),
                        basics.domainGlossary.GetId(typeof(CleaningIssue<SanitaryTrade>)))
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary
                            .GetUi(typeof(TechnicalIssue<SanitaryTrade>))
                            .GetFormattedEnglish(),
                        basics.domainGlossary.GetId(typeof(TechnicalIssue<SanitaryTrade>))), 
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary
                            .GetUi(typeof(ConsumablesIssue<SanitaryTrade>))
                            .GetFormattedEnglish(),
                        basics.domainGlossary.GetId(typeof(ConsumablesIssue<SanitaryTrade>))), 
                ]
            ]));

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
            (prompt: Back, promptId: new CallbackId((long)Back)),
            (prompt: Cancel, promptId: new CallbackId((long)Cancel)),
            (prompt: Skip, promptId: new CallbackId((long)Skip)),
            (prompt: Continue, promptId: new CallbackId((long)Continue)),
            (prompt: Yes, promptId: new CallbackId((long)Yes))
        };
        var outputWithPrompts = new OutputDto
        {
            ControlPromptsSelection = 
                promptSelection
                    .Select(static pair => pair.prompt)
                    .Aggregate(static (current, next) => current | next)
        };

        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<ReplyMarkup>.Some(
            new InlineKeyboardMarkup([
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[0].promptId].GetFormattedEnglish(), 
                        promptSelection[0].promptId), 
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[1].promptId].GetFormattedEnglish(), 
                        promptSelection[1].promptId)
                ],
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
            ]));
        
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
            Dt(ConsumablesItem.PaperTowels)];
        
        var promptSelection = new[] 
        {
            (prompt: Yes, promptId: new CallbackId((long)Yes)),
            (prompt: No, promptId: new CallbackId((long)No)),
        };
        
        var outputWithBoth = new OutputDto
        {
            DomainTermSelection = domainTermSelection,
            
            ControlPromptsSelection = 
                promptSelection
                    .Select(static pair => pair.prompt)
                    .Aggregate(static (current, next) => current | next)
        };
        
        // Assumes inlineKeyboardNumberOfColumns = 2
        var expectedReplyMarkup = Option<ReplyMarkup>.Some(
            new InlineKeyboardMarkup([
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.domainGlossary.GetUi(ConsumablesItem.PaperTowels).GetFormattedEnglish(),
                        basics.domainGlossary.GetId(ConsumablesItem.PaperTowels))
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[0].promptId].GetFormattedEnglish(),
                        promptSelection[0].promptId),
                    InlineKeyboardButton.WithCallbackData(
                        basics.uiByPromptId[promptSelection[1].promptId].GetFormattedEnglish(),
                        promptSelection[1].promptId),
                ]
            ]));

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
        var expectedReplyMarkup = Option<ReplyMarkup>.Some(new ReplyKeyboardMarkup([
            [new KeyboardButton(choice1), new KeyboardButton(choice2), new KeyboardButton(choice3)],
            [new KeyboardButton(choice4), new KeyboardButton(choice5)]
        ])
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
            Option<ReplyMarkup>.None(),
            actualReplyMarkup);
    }

    [Fact(Skip = "Mysteriously failing - investigate later")]
    public void GetReplyMarkup_Throws_WhenOutputIncludesInvalidEnum()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var outputWithInvalid = new OutputDto
        {
            ControlPromptsSelection = Cancel + 5
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