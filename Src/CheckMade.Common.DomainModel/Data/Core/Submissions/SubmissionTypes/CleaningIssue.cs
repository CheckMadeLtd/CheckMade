using System.Collections.Frozen;
using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using static CheckMade.Common.DomainModel.Data.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Common.DomainModel.Data.Core.Submissions.SubmissionTypes;

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