using System.Collections.Frozen;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;
using static CheckMade.Core.Model.Common.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Core.Model.Common.Submissions.SubmissionTypes;

public sealed record Assessment<T>(
    Guid Id,
    DateTimeOffset CreationDate,
    ISphereOfAction Sphere,
    AssessmentRating Rating,
    SubmissionEvidence Evidence,
    Role ReportedBy,
    IDomainGlossary Glossary) 
    : ISubmissionWithEvidence, ITradeSubmission<T> where T : ITrade, new()
{
    public IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary()
    {
        return new Dictionary<SubmissionSummaryCategories, UiString>
        {
            [CommonBasics] = SubmissionFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = SubmissionFormatters.FormatOperationalInfo(this, Glossary),
            [SubmissionTypeSpecificInfo] = UiConcatenate(
                Ui("<b>Assessment Rating:</b> "), 
                Glossary.GetUi(Rating),
                UiNewLines(1)),
            [EvidenceInfo] = SubmissionFormatters.FormatEvidenceInfo(this),
        }.ToFrozenDictionary();
    }
}