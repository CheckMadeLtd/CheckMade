using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Model.BotPrompts;
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
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidBotPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var converter = GetConverter(_services);
        
        // Arrange
        var prompt1 = new BotPrompt(UiNoTranslate("Prompt-1"), "p1");
        var prompt2 = new BotPrompt(UiNoTranslate("Prompt-2"), "p2");
        var prompt3 = new BotPrompt(UiNoTranslate("Prompt-3"), "p3");
        var prompt4 = new BotPrompt(UiNoTranslate("Prompt-4"), "p4");
        var prompt5 = new BotPrompt(UiNoTranslate("Prompt-5"), "p5");
        
        var fakeOutput = new OutputDto(
            UiNoTranslate(string.Empty),
            new[] { prompt1, prompt2, prompt3, prompt4, prompt5 },
            Option<IEnumerable<string>>.None());

        // Assumes inlineKeyboardNumberOfColumns = 2;
        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[] 
            { 
                new [] 
                { 
                    InlineKeyboardButton.WithCallbackData(prompt1.Text.GetFormattedEnglish(), prompt1.Id), 
                    InlineKeyboardButton.WithCallbackData(prompt2.Text.GetFormattedEnglish(), prompt2.Id) 
                },
                [
                    InlineKeyboardButton.WithCallbackData(prompt3.Text.GetFormattedEnglish(), prompt3.Id), 
                    InlineKeyboardButton.WithCallbackData(prompt4.Text.GetFormattedEnglish(), prompt4.Id)
                ],
                [
                    InlineKeyboardButton.WithCallbackData(prompt5.Text.GetFormattedEnglish(), prompt5.Id)
                ]
            }));
        
        // Act
        var actualReplyMarkup = converter.GetReplyMarkup(fakeOutput);
        
        // Assert
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
            Option<IEnumerable<BotPrompt>>.None(),
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
            Option<IEnumerable<BotPrompt>>.None(),
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