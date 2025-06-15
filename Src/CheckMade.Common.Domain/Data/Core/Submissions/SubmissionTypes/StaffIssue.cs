using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.UiTranslation;
using static CheckMade.Common.Domain.Data.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Common.Domain.Data.Core.Submissions.SubmissionTypes;

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