using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete;

public static class IssueFormatters
{
    public static UiString FormatBasicDetails<T>(ITradeIssue<T> issue, IDomainGlossary Glossary) where T : ITrade, new()
    {
        return UiConcatenate(
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiNoTranslate(issue.Sphere.Name), 
            UiNewLines(1),
            Ui("Reported by: "), UiNoTranslate(issue.ReportedBy.ByUser.FirstName));
    }
}


// Ui("Affected Facility: "), Glossary.GetUi(issue.Facility.GetType()),

// Ui("Description: "), issue.Evidence.Description.IsSome 
//     ? UiNoTranslate(issue.Evidence.Description.GetValueOrThrow())
//     : UiNoTranslate("n/a"),
// UiNewLines(1),
// Ui("# Attachments: "), Evidence.Media.IsSome
//     ? UiIndirect(Evidence.Media.GetValueOrThrow().Count.ToString())
//     : UiNoTranslate("n/a"),
