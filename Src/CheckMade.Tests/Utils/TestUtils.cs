using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Utils;

internal static class TestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";

    internal static string GetFirstRawEnglish(Result<IReadOnlyCollection<Output>> actualOutput) => 
        GetFirstRawEnglish(actualOutput.GetValueOrThrow());

    internal static string GetFirstRawEnglish(this IReadOnlyCollection<Output> actualOutput)
    {
        var text = actualOutput.First().Text.GetValueOrThrow();

        return text.Concatenations.Count > 0
            ? text.Concatenations.First()!.RawEnglishText
            : text.RawEnglishText;
    }

    internal static string GetAllRawEnglish(this IReadOnlyCollection<Output> actualOutput)
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

    internal static (IInputGenerator inputGenerator, IDomainGlossary glossary)
        GetBasicWorkflowTestingServices(IServiceProvider services)
    {
        return (
            services.GetRequiredService<IInputGenerator>(),
            services.GetRequiredService<IDomainGlossary>());
    }
}