using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete;

public static class IssueFormatters
{
    public static UiString FormatBasics<T>(ITradeIssue<T> issue)
        where T : ITrade, new()
    {
        return UiConcatenate(
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiIndirect(issue.Sphere.Name));
    }

    public static UiString FormatMetaInfo<T>(ITradeIssue<T> issue, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("Created: {0}", issue.CreationDate.ToString()),
            UiNewLines(1),
            Ui("Reported by: {0} ",
                $"{issue.ReportedBy.ByUser.FirstName} {issue.ReportedBy.ByUser.LastName}"),
            Ui("in their role as "), glossary.GetUi(issue.ReportedBy.RoleType.GetType()),
            UiNewLines(1),
            Ui("Currently handled by: "), issue.HandledBy.IsSome 
                ? UiConcatenate(UiIndirect(
                        $"{issue.HandledBy.GetValueOrThrow().ByUser.FirstName} " +
                        $"{issue.HandledBy.GetValueOrThrow().ByUser.LastName} "),
                    Ui("in their role as "), glossary.GetUi(issue.HandledBy.GetValueOrThrow().RoleType.GetType()))
                : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("Current status: {0}", issue.Status.ToString()));
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
