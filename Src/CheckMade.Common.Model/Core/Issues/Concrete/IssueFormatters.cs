using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete;

internal static class IssueFormatters
{
    public static UiString FormatCommonBasics<T>(
        ITradeIssue<T> issue, IDomainGlossary glossary) where T : ITrade, new()
    {
        return UiConcatenate(
            Ui("Trade: "), glossary.GetUi(typeof(T)),
            UiNewLines(1),
            Ui("Issue type: "), glossary.GetUi(issue.GetType()),
            UiNewLines(1),
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiIndirect(issue.Sphere.Name),
            UiNewLines(1));
    }

    public static UiString FormatOperationalInfo<T>(
        ITradeIssue<T> issue, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("Created: {0}", issue.CreationDate.ToString("u")),
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
            Ui("Current status: "), glossary.GetUi(issue.Status),
            UiNewLines(1));
    }

    public static UiString FormatFacilityInfo<T>(
        ITradeIssueInvolvingFacility<T> issue, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("Affected facility: "), glossary.GetUi(issue.Facility.GetType()),
            UiNewLines(1));
    }

    public static UiString FormatEvidenceInfo<T>(
        ITradeIssueWithEvidence<T> issue) where T : ITrade
    {
        var media = issue.Evidence.Media.IsSome
            ? issue.Evidence.Media.GetValueOrThrow()
            : null;

        var mediaCaptions = media != null
            ? media
                .Where(m => m.Caption.IsSome)
                .Select(m => UiConcatenate(
                    UiNewLines(1),
                    UiIndirect($"> {m.Caption.GetValueOrThrow()}")))
                .ToImmutableReadOnlyCollection()
            : [];
        
        return UiConcatenate(
            Ui("Description: "), issue.Evidence.Description.IsSome 
                ? UiConcatenate(
                    UiIndirect(issue.Evidence.Description.GetValueOrThrow()),
                    UiConcatenate(mediaCaptions))
                : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("# Attachments: "), media != null
                ? UiIndirect(media.Count.ToString())
                : UiNoTranslate("n/a"));
    }
}