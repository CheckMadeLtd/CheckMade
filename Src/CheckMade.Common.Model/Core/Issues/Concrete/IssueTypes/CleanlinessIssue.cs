using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record CleanlinessIssue<T>(
        Guid Id,
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IFacility Facility,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status,
        IDomainGlossary Glossary) 
    : ITradeIssue<T>, IIssueInvolvingFacility, IIssueWithEvidence where T : ITrade, new()
{
    public UiString GetSummary()
    {
        return UiConcatenate(
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiNoTranslate(Sphere.Name),
            Ui("Affected Facility: "), Glossary.GetUi(Facility.GetType()));
    }
}