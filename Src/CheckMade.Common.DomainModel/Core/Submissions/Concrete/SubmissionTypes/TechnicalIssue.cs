using CheckMade.Common.DomainModel.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.DomainModel.Core.LiveEvents;
using CheckMade.Common.DomainModel.Core.Trades;
using CheckMade.Common.DomainModel.Utils;
using static CheckMade.Common.DomainModel.Core.Submissions.Concrete.SubmissionSummaryCategories;

namespace CheckMade.Common.DomainModel.Core.Submissions.Concrete.SubmissionTypes;

public sealed record TechnicalIssue<T>(
    Guid Id,    
    DateTimeOffset CreationDate,
    ISphereOfAction Sphere,
    IFacility Facility,
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
            [EvidenceInfo] = SubmissionFormatters.FormatEvidenceInfo(this)
        };
    }
}