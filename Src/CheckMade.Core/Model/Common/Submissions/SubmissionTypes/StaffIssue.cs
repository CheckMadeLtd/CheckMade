using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;
using static CheckMade.Core.Model.Common.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Core.Model.Common.Submissions.SubmissionTypes;

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