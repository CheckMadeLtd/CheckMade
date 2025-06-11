using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Core.Submissions.Concrete.IssueSummaryCategories;

namespace CheckMade.Common.Model.Core.Submissions.Concrete.IssueTypes;

public sealed record TechnicalIssue<T>(
        Guid Id,    
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IFacility Facility,
        SubmissionEvidence Evidence,
        Role ReportedBy,
        Option<Role> HandledBy,
        IssueStatus Status,
        IDomainGlossary Glossary) 
    : ITradeSubmissionInvolvingFacility<T>, ISubmissionWithEvidence where T : ITrade, new()
{
    public IReadOnlyDictionary<IssueSummaryCategories, UiString> GetSummary()
    {
        return new Dictionary<IssueSummaryCategories, UiString>
        {
            [CommonBasics] = IssueFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = IssueFormatters.FormatOperationalInfo(this, Glossary),
            [FacilityInfo] = IssueFormatters.FormatFacilityInfo(this, Glossary),
            [EvidenceInfo] = IssueFormatters.FormatEvidenceInfo(this)
        };
    }
}