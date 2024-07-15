using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Output;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Utils;

internal static class TestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";

    internal static string GetFirstRawEnglish(Result<IReadOnlyCollection<OutputDto>> actualOutput) => 
        GetFirstRawEnglish(actualOutput.GetValueOrThrow());

    internal static string GetFirstRawEnglish(IReadOnlyCollection<OutputDto> actualOutput)
    {
        var text = actualOutput.First().Text.GetValueOrThrow();

        return text.Concatenations.Count > 0
            ? text.Concatenations.First()!.RawEnglishText
            : text.RawEnglishText;
    }

    internal static (ITlgInputGenerator inputGenerator, IDomainGlossary glossary)
        GetBasicWorkflowTestingServices(IServiceProvider services)
    {
        return (
            services.GetRequiredService<ITlgInputGenerator>(),
            services.GetRequiredService<IDomainGlossary>());
    }
}