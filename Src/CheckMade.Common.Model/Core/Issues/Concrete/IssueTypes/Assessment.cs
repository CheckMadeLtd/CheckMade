using System.Collections.Frozen;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Core.Issues.Concrete.IssueSummaryCategories;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record Assessment<T>(
    Guid Id,
    DateTimeOffset CreationDate,
    ISphereOfAction Sphere,
    IFacility Facility,
    AssessmentRating Rating,
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
            [IssueSpecificInfo] = UiConcatenate(
                Ui("<b>Assessment Rating:</b> "), 
                Glossary.GetUi(Rating),
                UiNewLines(1)),
            [EvidenceInfo] = IssueFormatters.FormatEvidenceInfo(this),
        }.ToFrozenDictionary();
    }
}