using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckMade.Tests.Unit.Common;

public class UiStringAndTranslationTests
{
    // Various tests just of the UiString class: test UiConcatenate with multiple Ui() that each make use of params; 
    // test and document each what UiNoTranslate and UiIndirect and GetFormattedEnglish do; 

    // Various tests of Translate() with: missing key; missing dictionary; correct handling of /n and //n; 
    // no and too few and too many message params (and expected error message); mix of UiNoTranslate(), UiIndirect() and normal Ui() in a single UiString record;
    // correct handling of other special characters;
    
    // Test UiTranslatorFactory briefly (check against duplicate keys)

    private ServiceProvider? _services;

    [Fact]
    public void Translate_ReturnsCorrectValue_WhenKeyPresentInTargetLanguageResourceFile()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var mockUiTranslatorFactory = new Mock<IUiTranslatorFactory>();
        var logger = _services.GetRequiredService<ILogger<UiTranslator>>();
        const LanguageCode langCode = LanguageCode.De;

        const string enKey = " English key with param {0} and \n linebreak and leading space.";
        const string trans = " Deutscher Schlüssel mit Parameter {0} und \n Zeilenumbruch und Leerzeichen am Anfang."; 
        const string param1 = "param1";
        
        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { "key1", "translation1" },
                { enKey, trans },
                { "key3", "translation3" }
            });
        
        mockUiTranslatorFactory
            .Setup(f => f.Create(langCode))
            .Returns(new UiTranslator(fakeTranslationByKey, logger));
        var uiTranslator = mockUiTranslatorFactory.Object.Create(langCode);

        var uiString = Ui(enKey, param1);

        // Act
        var translatedValue = uiTranslator.Translate(uiString);

        // Assert
        translatedValue.Should().Be(string.Format(trans, param1));
    }
    
    [Fact]
    public void GetFormattedEnglish_ReturnsCorrectResult()
    {
        const string param1 = "param1", param2 = "param2";
        var uiString = Ui("This is a test message with {0} and {1}.", param1, param2);
        const string expected = $"This is a test message with {param1} and {param2}.";

        uiString.GetFormattedEnglish().Should().Be(expected);
    }
}