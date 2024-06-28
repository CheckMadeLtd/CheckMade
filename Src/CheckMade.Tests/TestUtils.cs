using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.Tests;

internal static class TestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";

    internal static string GetFirstRawEnglish(Result<IReadOnlyCollection<OutputDto>> actualOutput) => GetFirstRawEnglish(actualOutput.GetValueOrThrow());

    internal static string GetFirstRawEnglish(IReadOnlyCollection<OutputDto> actualOutput)
    {
        var text = actualOutput.First().Text.GetValueOrThrow();

        return text.Concatenations.Count > 0
            ? text.Concatenations.First()!.RawEnglishText
            : text.RawEnglishText;
    }
}