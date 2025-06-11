using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Core.Submissions.Concrete.SubmissionSummaryCategories;

namespace CheckMade.Common.Model.Core.Submissions.Concrete.IssueTypes;

public sealed record GeneralIssue<T>(
    Guid Id, 
    DateTimeOffset CreationDate, 
    ISphereOfAction Sphere, 
    SubmissionEvidence Evidence, 
    Role ReportedBy, 
    IDomainGlossary Glossary) 
    : ITradeSubmission<T>, ISubmissionWithEvidence where T : ITrade, new()
{
    public IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary()
    {
        return new Dictionary<SubmissionSummaryCategories, UiString>
        {
            [CommonBasics] = IssueFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = IssueFormatters.FormatOperationalInfo(this, Glossary),
            [EvidenceInfo] = IssueFormatters.FormatEvidenceInfo(this)
        };
    }
}