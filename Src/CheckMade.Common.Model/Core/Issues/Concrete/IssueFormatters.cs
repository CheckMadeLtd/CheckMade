using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete;

internal static class IssueFormatters
{
    public static UiString FormatCommonBasics<T>(
        ITradeIssue<T> issue, IDomainGlossary glossary) where T : ITrade, new()
    {
        return UiConcatenate(
            Ui("<b>Trade:</b> "), glossary.GetUi(typeof(T)),
            UiNewLines(1),
            Ui("<b>Issue type:</b> "), glossary.GetUi(issue.GetType()),
            UiNewLines(1),
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiIndirect(issue.Sphere.Name),
            UiNewLines(1));
    }

    public static UiString FormatOperationalInfo<T>(
        ITradeIssue<T> issue, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("<b>Created:</b> {0}", issue.CreationDate.ToString("u")),
            UiNewLines(1),
            Ui("<b>Reported by:</b> {0} ",
                $"{issue.ReportedBy.ByUser.FirstName} {issue.ReportedBy.ByUser.LastName}"),
            Ui("in their role as "), glossary.GetUi(issue.ReportedBy.RoleType.GetType()),
            UiNewLines(1),
            Ui("<b>Currently handled by:</b> "), issue.HandledBy.IsSome 
                ? UiConcatenate(UiIndirect(
                        $"{issue.HandledBy.GetValueOrThrow().ByUser.FirstName} " +
                        $"{issue.HandledBy.GetValueOrThrow().ByUser.LastName} "),
                    Ui("in their role as "), glossary.GetUi(issue.HandledBy.GetValueOrThrow().RoleType.GetType()))
                : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("<b>Current status:</b> "), glossary.GetUi(issue.Status),
            UiNewLines(1));
    }

    public static UiString FormatFacilityInfo<T>(
        ITradeIssueInvolvingFacility<T> issue, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("<b>Affected facility:</b> "), glossary.GetUi(issue.Facility.GetType()),
            UiNewLines(1));
    }

    public static UiString FormatEvidenceInfo(ITradeIssueWithEvidence issue)
    {
        var attachments = issue.Evidence.Attachments.IsSome
            ? issue.Evidence.Attachments.GetValueOrThrow()
            : null;

        var captions = attachments != null
            ? attachments
                .Where(m => m.Caption.IsSome)
                .Select(m => UiConcatenate(
                    UiNewLines(1),
                    UiIndirect($"> {m.Caption.GetValueOrThrow()}")))
                .ToImmutableReadOnlyCollection()
            : [];
        
        return UiConcatenate(
            Ui("<b>Description:</b> "), 
            issue.Evidence.Description.IsSome 
                ? UiConcatenate(
                    UiIndirect(issue.Evidence.Description.GetValueOrThrow()),
                    UiConcatenate(captions))
                : captions.Count > 0 
                    ? UiConcatenate(captions)
                    : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("<b># Attachments:</b> "), attachments != null
                ? UiIndirect(attachments.Count.ToString())
                : UiNoTranslate("n/a"));
    }
}