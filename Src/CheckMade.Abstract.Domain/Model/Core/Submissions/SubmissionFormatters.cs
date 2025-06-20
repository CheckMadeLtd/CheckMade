using CheckMade.Abstract.Domain.Model.Core.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions;

internal static class SubmissionFormatters
{
    public static UiString FormatCommonBasics<T>(
        ITradeSubmission<T> submission, IDomainGlossary glossary) where T : ITrade, new()
    {
        return UiConcatenate(
            Ui("<b>Trade:</b> "), glossary.GetUi(typeof(T)),
            UiNewLines(1),
            Ui("<b>Submission type:</b> "), glossary.GetUi(submission.GetType()),
            UiNewLines(1),
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiIndirect(submission.Sphere.Name),
            UiIndirect(submission.Sphere.Details.LocationName.IsSome
                ? " - " + submission.Sphere.Details.LocationName.GetValueOrDefault()
                : string.Empty),
            UiNewLines(1));
    }

    public static UiString FormatOperationalInfo<T>(
        ITradeSubmission<T> submission, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("<b>Created:</b> {0}", submission.CreationDate.ToString("u")),
            UiNewLines(1),
            Ui("<b>Reported by:</b> {0} ",
                $"{submission.ReportedBy.ByUser.FirstName} {submission.ReportedBy.ByUser.LastName}"),
            Ui("in their role as "), glossary.GetUi(submission.ReportedBy.RoleType.GetType()),
            UiNewLines(1));
        // ToDo: put back in when I implemented setting HandledBy as part of task management workflow
        // Ui("<b>Currently handled by:</b> "), issue.HandledBy.IsSome 
        //     ? UiConcatenate(UiIndirect(
        //             $"{issue.HandledBy.GetValueOrThrow().ByUser.FirstName} " +
        //             $"{issue.HandledBy.GetValueOrThrow().ByUser.LastName} "),
        //         Ui("in their role as "), glossary.GetUi(issue.HandledBy.GetValueOrThrow().RoleType.GetType()))
        //     : UiNoTranslate("n/a"),
        // UiNewLines(1),
        // Ui("<b>Current status:</b> "), glossary.GetUi(issue.Status),
        // UiNewLines(1));
    }

    public static UiString FormatFacilityInfo<T>(
        ITradeSubmissionInvolvingFacility<T> submission, IDomainGlossary glossary) where T : ITrade
    {
        return UiConcatenate(
            Ui("<b>Affected facility:</b> "), glossary.GetUi(submission.Facility.GetType()),
            UiNewLines(1));
    }

    public static UiString FormatEvidenceInfo(ISubmissionWithEvidence submission)
    {
        var attachments = submission.Evidence.Attachments.IsSome
            ? submission.Evidence.Attachments.GetValueOrThrow()
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
            submission.Evidence.Description.IsSome 
                ? UiConcatenate(
                    UiIndirect(submission.Evidence.Description.GetValueOrThrow()),
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