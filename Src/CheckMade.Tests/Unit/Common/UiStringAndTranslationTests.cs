using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckMade.Tests.Unit.Common;

public class UiStringAndTranslationTests
{
    private ServiceProvider? _services;

    [Fact]
    public void Translate_ReturnsCorrectValue_ForKeyWithParamsAndLineBreak()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();

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
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));
        
        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        var uiString = Ui(enKey, param1);

        // Act
        var actualTranslation = uiTranslator.Translate(uiString);

        // Assert
        actualTranslation.Should().Be(string.Format(trans, param1));
    }

    [Fact]
    public void Translate_ReturnsCorrectValue_ForConcatWithParamsAndNoTranslateAndIndirect()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();
        
        const string enKey1 = "Key1 with {0}";
        const string enKey2 = "Key2 with {0}";
        const string enKey3 = "Const value to test UiIndirect";
        const string trans1 = "Schlüssel1 mit {0}";
        const string trans2 = "Schlüssel2 mit {0}";
        const string trans3 = "Konstanter Wert, um UiIndirect zu testen";
        const string param1 = "param1";
        const string param2 = "param2";

        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { enKey1, trans1 },
                { enKey2, trans2 },
                { enKey3, trans3 }
            });
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));
        
        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);

        var uiString = UiConcatenate(
            Ui(enKey1, param1),
            UiNoTranslate(" "),
            Ui(enKey2, param2),
            UiNoTranslate(" "),
            UiIndirect(enKey3));

        // Act
        var actualTranslation = uiTranslator.Translate(uiString);

        // Assert
        actualTranslation.Should().Be(
            $"{string.Format(trans1, param1)} {string.Format(trans2, param2)} {trans3}");
    }

    [Fact]
    public void Translate_DoesNotTranslateUiNoTranslate_EvenIfKeyValuePairPresent()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();
        
        const string enKey = "Key is present";
        const string trans = "Der Schlüssel ist present";
        
        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { enKey, trans },
            });
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));
        
        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        var uiStringNoTranslate = UiNoTranslate(enKey);
        
        // Act
        var expectedNotTranslated = uiTranslator.Translate(uiStringNoTranslate);
        
        // Assert
        expectedNotTranslated.Should().Be(enKey);
    }

    [Fact]
    public void Translate_ReturnsUnformattedTranslation_ForInputWithMissingParams()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();

        const string enKey = "Key with {0} and {1} as two params.";
        const string trans = "Schlüssel mit {0} und {1} als zwei Parameter.";

        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { enKey, trans }
            });
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));
        
        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        // ReSharper disable once FormatStringProblem
        var uiString = Ui(enKey);

        // Act
        var actualTranslation = uiTranslator.Translate(uiString);

        // Assert
        actualTranslation.Should().Be($"{trans}[]");
    }
    
    [Fact]
    public void Translate_ReturnsUnformattedTranslationWithAppendedParams_ForInputWithSomeButTooFewParams()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();

        const string enKey = "Key with {0} and {1} as two params.";
        const string trans = "Schlüssel mit {0} und {1} als zwei Parameter.";
        const string param1 = "param1";
        
        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { enKey, trans }
            });
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));
        
        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        // ReSharper disable once FormatStringProblem
        var uiString = Ui(enKey, param1);

        // Act
        var actualTranslation = uiTranslator.Translate(uiString);

        // Assert
        actualTranslation.Should().Be($"{trans}[{string.Join("; ", uiString.MessageParams)}]");
    }
    
    [Fact]
    public void Translate_ReturnsFullyFormattedTranslationPlusListOfExcessParams_ForInputWithTooManyParams()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();

        const string enKey = "Key with {0} as only param.";
        const string trans = "Schlüssel mit {0} als einziger Parameter.";
        const string param1 = "param1";
        const string param2 = "param2";
        const string param3 = "param3";
        
        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { enKey, trans }
            });
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));
        
        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        // ReSharper disable once FormatStringProblem
        var uiString = Ui(enKey, param1, param2, param3);

        // Act
        var actualTranslation = uiTranslator.Translate(uiString);

        // Assert
        actualTranslation.Should().Be(
            $"{string.Format(trans, param1)}[{string.Join("; ", uiString.MessageParams.TakeLast(2))}]");
    }

    [Fact]
    public void Translate_ReturnsFormattedEnglish_WhenTargetLanguageIsNotEnglish_ButTranslationDictionaryMissing()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();

        const string enKey = "My English {0} key with param.";
        const string param1 = "param1";
        
        var fakeTranslationByKey = Option<IDictionary<string, string>>.None();
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));

        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        var uiString = Ui(enKey, param1);
        
        // Act
        var actualTranslation = uiTranslator.Translate(uiString);
        
        // Assert
        actualTranslation.Should().Be(string.Format(enKey, param1));
    }
    
    [Fact]
    public void Translate_ReturnsFormattedEnglish_WhenTargetLanguageDictionaryPresent_ButKeyMissing()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = GetUiTranslatorFactoryWithBasicDependencies();

        const string enString = "My English {0} string with one param.";
        const string param1 = "param1";
        
        var fakeTranslationByKey = Option<IDictionary<string, string>>.Some(
            new Dictionary<string, string>
            {
                { "some value but not our string", "some irrelevant translation" }
            });
        
        factory.mockFactory
            .Setup(f => f.Create(factory.deLangCode))
            .Returns(new UiTranslator(fakeTranslationByKey, factory.logger));

        var uiTranslator = factory.mockFactory.Object.Create(factory.deLangCode);
        var uiString = Ui(enString, param1);
        
        // Act
        var actualTranslation = uiTranslator.Translate(uiString);
        
        // Assert
        actualTranslation.Should().Be(string.Format(enString, param1));
    }
    
    [Fact]
    public void Translate_ReturnsFormattedEnglish_WhenTargetLanguageIsEnglish()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var factory = _services.GetRequiredService<IUiTranslatorFactory>();

        const string enKey = "My English {0} key with param.";
        const string param1 = "param1";
        
        var uiTranslator = factory.Create(LanguageCode.En);
        var uiString = Ui(enKey, param1);
        
        // Act
        var actualTranslation = uiTranslator.Translate(uiString);
        
        // Assert
        actualTranslation.Should().Be(string.Format(enKey, param1));
    }
    
    [Fact]
    public void GetFormattedEnglish_ReturnsCorrectResult()
    {
        const string param1 = "param1", param2 = "param2";
        var uiString = Ui("This is a test message with {0} and {1}.", param1, param2);
        const string expected = $"This is a test message with {param1} and {param2}.";

        uiString.GetFormattedEnglish().Should().Be(expected);
    }

    private (Mock<IUiTranslatorFactory> mockFactory, ILogger<UiTranslator> logger, LanguageCode deLangCode) 
        GetUiTranslatorFactoryWithBasicDependencies() =>
        (new Mock<IUiTranslatorFactory>(),
            _services!.GetRequiredService<ILogger<UiTranslator>>(),
            LanguageCode.De);
}