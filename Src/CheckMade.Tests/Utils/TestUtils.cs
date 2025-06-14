using CheckMade.Common.DomainModel.ChatBot.Output;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Utils;

internal static class TestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";

    internal static string GetFirstRawEnglish(Result<IReadOnlyCollection<OutputDto>> actualOutput) => 
        GetFirstRawEnglish(actualOutput.GetValueOrThrow());

    internal static string GetFirstRawEnglish(this IReadOnlyCollection<OutputDto> actualOutput)
    {
        var text = actualOutput.First().Text.GetValueOrThrow();

        return text.Concatenations.Count > 0
            ? text.Concatenations.First()!.RawEnglishText
            : text.RawEnglishText;
    }

    internal static string GetAllRawEnglish(this IReadOnlyCollection<OutputDto> actualOutput)
    {
        var combinedRawEnglish = string.Empty;

        foreach (var output in actualOutput)
        {
            var uiString = output.Text;

            if (uiString.IsSome)
            {
                var concatenations = uiString.GetValueOrThrow().Concatenations;
                
                if (concatenations.Count > 0)
                {
                    combinedRawEnglish = $"{combinedRawEnglish}; " +
                                         $"{string.Join("; ", 
                                             concatenations.Select(static c => c!.RawEnglishText))}";
                }
                else
                {
                    combinedRawEnglish = $"{combinedRawEnglish}; " +
                                         $"{uiString.GetValueOrThrow().RawEnglishText}";
                }
            }
        }

        return combinedRawEnglish;
    }

    internal static (ITlgInputGenerator inputGenerator, IDomainGlossary glossary)
        GetBasicWorkflowTestingServices(IServiceProvider services)
    {
        return (
            services.GetRequiredService<ITlgInputGenerator>(),
            services.GetRequiredService<IDomainGlossary>());
    }
}