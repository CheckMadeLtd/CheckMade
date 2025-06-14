using CheckMade.Common.DomainModel.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.DomainModel.Interfaces.Core;
using static CheckMade.Common.DomainModel.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Common.DomainModel.Core.Submissions.SubmissionTypes;

public sealed record StaffIssue<T>(
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
            [CommonBasics] = SubmissionFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = SubmissionFormatters.FormatOperationalInfo(this, Glossary),
            [EvidenceInfo] = SubmissionFormatters.FormatEvidenceInfo(this)
        };
    }
}