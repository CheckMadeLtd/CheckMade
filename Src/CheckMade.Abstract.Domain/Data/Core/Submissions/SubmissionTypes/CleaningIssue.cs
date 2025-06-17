using System.Collections.Frozen;
using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using General.Utils.UiTranslation;
using static CheckMade.Abstract.Domain.Data.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Abstract.Domain.Data.Core.Submissions.SubmissionTypes;

public sealed record CleaningIssue<T>(
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
        }.ToFrozenDictionary();
    }
}