using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Model.ControlPrompt;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Tests.Unit.Telegram;

public class OutputToReplyMarkupConverterTests
{
    private ServiceProvider? _services;

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidControlPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var converter = GetConverter(_services);
        var promptUiById = new ControlPromptsProvider().UiById;
        var promptSelection = 
            new[]
            {
                (prompt: ControlPrompts.No, promptId: new ControlPromptCallbackId((int)ControlPrompts.No)),
                (prompt: ControlPrompts.Yes, promptId: new ControlPromptCallbackId((int)ControlPrompts.Yes)),
                (prompt: ControlPrompts.Bad, promptId: new ControlPromptCallbackId((int)ControlPrompts.Bad)),
                (prompt: ControlPrompts.Ok, promptId: new ControlPromptCallbackId((int)ControlPrompts.Ok)),
                (prompt: ControlPrompts.Good, promptId: new ControlPromptCallbackId((int)ControlPrompts.Good))
            };
        var fakeOutput = new OutputDto(
            UiNoTranslate(string.Empty),
            promptSelection.Select(pair => pair.prompt).ToArray(),
            Option<IEnumerable<string>>.None());

        // Assumes inlineKeyboardNumberOfColumns = 2;
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[] 
            { 
                new [] 
                { 
                    InlineKeyboardButton.WithCallbackData(
                        promptUiById[promptSelection[0].promptId].GetFormattedEnglish(), 
                        promptSelection[0].promptId.Id), 
                    InlineKeyboardButton.WithCallbackData(
                        promptUiById[promptSelection[1].promptId].GetFormattedEnglish(), 
                        promptSelection[1].promptId.Id), 
                },
                [
                    InlineKeyboardButton.WithCallbackData(
                        promptUiById[promptSelection[2].promptId].GetFormattedEnglish(), 
                        promptSelection[2].promptId.Id), 
                    InlineKeyboardButton.WithCallbackData(
                        promptUiById[promptSelection[3].promptId].GetFormattedEnglish(), 
                        promptSelection[3].promptId.Id), 
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        promptUiById[promptSelection[4].promptId].GetFormattedEnglish(), 
                        promptSelection[4].promptId.Id), 
                ]
            }));
        
        var actualReplyMarkup = converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(expectedReplyMarkup.GetValueOrDefault(), actualReplyMarkup.GetValueOrDefault());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedReplyKeyboard_ForValidPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var converter = GetConverter(_services);
        
        // Arrange
        const string choice1 = "c1";
        const string choice2 = "c2";
        const string choice3 = "c3";
        const string choice4 = "c4";
        const string choice5 = "c5";

        var fakeOutput = new OutputDto(
            UiNoTranslate(string.Empty),
            Option<IEnumerable<ControlPrompts>>.None(),
            new[] { choice1, choice2, choice3, choice4, choice5 });
        
        // Assumes replyKeyboardNumberOfColumns = 3
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(new ReplyKeyboardMarkup(new[]
        {
            new[] 
                { new KeyboardButton(choice1), new KeyboardButton(choice2), new KeyboardButton(choice3) },
                [ new KeyboardButton(choice4), new KeyboardButton(choice5) ]
        }));
        
        // Act
        var actualReplyMarkup = converter.GetReplyMarkup(fakeOutput);
        
        // Assert
        Assert.Equivalent(expectedReplyMarkup.GetValueOrDefault(), actualReplyMarkup.GetValueOrDefault());
    }

    [Fact]
    public void GetReplyMarkup_ReturnsNone_ForOutputWithoutPromptsOrPredefinedChoices()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var converter = GetConverter(_services);
        
        var fakeOutput = new OutputDto(
            UiNoTranslate(string.Empty),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());
        
        var actualReplyMarkup = converter.GetReplyMarkup(fakeOutput);
        
        Assert.Equivalent(Option<IReplyMarkup>.None(), actualReplyMarkup);
    }

    private static IOutputToReplyMarkupConverter GetConverter(IServiceProvider sp)
    {
        var converterFactory = sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>();
        
        return converterFactory.Create(new UiTranslator(
            Option<IReadOnlyDictionary<string, string>>.None(),
            sp.GetRequiredService<ILogger<UiTranslator>>()));
    }
}