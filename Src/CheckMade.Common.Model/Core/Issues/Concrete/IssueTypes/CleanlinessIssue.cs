using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record CleanlinessIssue<T>(
        Guid Id,
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IFacility Facility,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status) 
    : ITradeIssue<T>, IIssueInvolvingFacility, IIssueWithEvidence where T : ITrade
{
    public UiString GetSummary()
    {
        // ToDo: Add new UiStrings to translations
        return UiConcatenate(
            Ui("Summary of {0}:\n", GetType().Name),
            Ui("Reported by a: "),
            UiNoTranslate(ReportedBy.RoleType.GetType().Name));
    }
}