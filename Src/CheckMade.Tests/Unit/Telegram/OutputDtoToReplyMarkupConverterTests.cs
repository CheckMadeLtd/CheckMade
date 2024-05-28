using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Model.BotPrompts;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Tests.Unit.Telegram;

public class OutputDtoToReplyMarkupConverterTests
{
    private ServiceProvider? _services;

    [Fact]
    public void GetReplyMarkup_ReturnsCorrectlyArrangedInlineKeyboard_ForValidBotPrompts()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var converterFactory = _services.GetRequiredService<IOutputDtoToReplyMarkupConverterFactory>();
        var converter = converterFactory.Create(new UiTranslator(
            Option<IReadOnlyDictionary<string, string>>.None(),
            _services.GetRequiredService<ILogger<UiTranslator>>()));

        var prompt1A = new BotPrompt(UiNoTranslate("Prompt-1a"), "p1a");
        var prompt1B = new BotPrompt(UiNoTranslate("Prompt-1b"), "p1b");
        var prompt2A = new BotPrompt(UiNoTranslate("Prompt-2a"), "p2a");
        var prompt2B = new BotPrompt(UiNoTranslate("Prompt-2b"), "p2b");
        var prompt3A = new BotPrompt(UiNoTranslate("Prompt-3a"), "p3a");
        
        var fakeOutput = new OutputDto(
            UiNoTranslate(string.Empty),
            Option<IEnumerable<BotPrompt>>.Some(new[] { prompt1A, prompt1B, prompt2A, prompt2B, prompt3A }),
            Option<IEnumerable<string>>.None());

        var expectedReplyMarkup = Option<IReplyMarkup>.Some(
            new InlineKeyboardMarkup(new[] 
            { 
                new [] 
                { 
                    InlineKeyboardButton.WithCallbackData(prompt1A.Text.GetFormattedEnglish(), prompt1A.Id), 
                    InlineKeyboardButton.WithCallbackData(prompt1B.Text.GetFormattedEnglish(), prompt1B.Id) 
                },
                [
                    InlineKeyboardButton.WithCallbackData(prompt2A.Text.GetFormattedEnglish(), prompt2A.Id), 
                    InlineKeyboardButton.WithCallbackData(prompt2B.Text.GetFormattedEnglish(), prompt2B.Id)
                ],
                [
                    InlineKeyboardButton.WithCallbackData(prompt3A.Text.GetFormattedEnglish(), prompt3A.Id)
                ]
            }));
        
        // Act
        var actualReplyMarkup = converter.GetReplyMarkup(fakeOutput);
        
        // Assert
        actualReplyMarkup.Should().BeEquivalentTo(expectedReplyMarkup);
    }
}