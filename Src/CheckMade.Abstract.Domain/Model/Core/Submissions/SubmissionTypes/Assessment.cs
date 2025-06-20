using System.Collections.Frozen;
using CheckMade.Abstract.Domain.Model.Core.Actors;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Abstract.Domain.Model.Core.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;
using static CheckMade.Abstract.Domain.Model.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions.SubmissionTypes;

public sealed record Assessment<T>(
    Guid Id,
    DateTimeOffset CreationDate,
    ISphereOfAction Sphere,
    IFacility Facility,
    AssessmentRating Rating,
    SubmissionEvidence Evidence,
    Role ReportedBy,
    IDomainGlossary Glossary) 
    : ITradeSubmissionInvolvingFacility<T>, ISubmissionWithEvidence where T : ITrade, new()
{
    public IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary()
    {
        return new Dictionary<SubmissionSummaryCategories, UiString>
        {
            [CommonBasics] = SubmissionFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = SubmissionFormatters.FormatOperationalInfo(this, Glossary),
            [FacilityInfo] = SubmissionFormatters.FormatFacilityInfo(this, Glossary),
            [SubmissionTypeSpecificInfo] = UiConcatenate(
                Ui("<b>Assessment Rating:</b> "), 
                Glossary.GetUi(Rating),
                UiNewLines(1)),
            [EvidenceInfo] = SubmissionFormatters.FormatEvidenceInfo(this),
        }.ToFrozenDictionary();
    }
}