using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;

internal static class AssessmentFormatters
{
    public static UiString FormatCommonBasics(
        Assessment assessment, IDomainGlossary glossary)
    {
        return UiConcatenate(
            Ui("<b>Trade:</b> "), glossary.GetUi(typeof(SanitaryTrade)),
            UiNewLines(1),
            new SanitaryTrade().GetSphereOfActionLabel, UiNoTranslate(": "), UiIndirect(assessment.Sphere.Name),
            UiNewLines(1),
            Ui("<b>Rating:</b> "), glossary.GetUi(assessment.Rating),
            UiNewLines(1));
    }

    public static UiString FormatOperationalInfo(
        Assessment assessment, IDomainGlossary glossary)
    {
        return UiConcatenate(
            Ui("<b>Created:</b> {0}", assessment.CreationDate.ToString("u")),
            UiNewLines(1),
            Ui("<b>Reported by:</b> {0} ",
                $"{assessment.ReportedBy.ByUser.FirstName} {assessment.ReportedBy.ByUser.LastName}"),
            Ui("in their role as "), glossary.GetUi(assessment.ReportedBy.RoleType.GetType()),
            UiNewLines(1));
    }

    public static UiString FormatFacilityInfo(
        Assessment assessment, IDomainGlossary glossary)
    {
        return UiConcatenate(
            Ui("<b>Affected facility:</b> "), glossary.GetUi(assessment.Facility.GetType()),
            UiNewLines(1));
    }

    public static UiString FormatEvidenceInfo(Assessment assessment)
    {
        var attachments = assessment.Evidence.IsSome 
            ? assessment.Evidence.GetValueOrThrow().Attachments.IsSome
                ? assessment.Evidence.GetValueOrThrow().Attachments.GetValueOrThrow()
                : null
            : null;

        var captions = attachments != null
            ? attachments
                .Where(static m => m.Caption.IsSome)
                .Select(static m => UiConcatenate(
                    UiNewLines(1),
                    UiIndirect($"> {m.Caption.GetValueOrThrow()}")))
                .ToList()
            : [];
        
        return UiConcatenate(
            Ui("<b>Description:</b> "), 
            assessment.Evidence.IsSome 
                ? assessment.Evidence.GetValueOrThrow().Description.IsSome 
                    ? UiConcatenate(
                        UiIndirect(assessment.Evidence.GetValueOrThrow().Description.GetValueOrThrow()),
                        UiConcatenate(captions))
                    : captions.Count > 0 
                        ? UiConcatenate(captions)
                        : UiNoTranslate("n/a") 
                : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("<b># Attachments:</b> "), attachments != null
                ? UiIndirect(attachments.Count.ToString())
                : UiNoTranslate("n/a"));
    }
}