using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
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
        Role ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status,
        IDomainGlossary Glossary) 
    : ITradeIssue<T>, IIssueInvolvingFacility, IIssueWithEvidence where T : ITrade, new()
{
    public UiString FormatDetails()
    {
        return UiConcatenate(
            new T().GetSphereOfActionLabel, UiNoTranslate(": "), UiNoTranslate(Sphere.Name), 
            UiNewLines(1),
            Ui("Affected Facility: "), Glossary.GetUi(Facility.GetType()),
            UiNewLines(1),
            Ui("Description: "), Evidence.Description.IsSome 
                ? UiNoTranslate(Evidence.Description.GetValueOrThrow())
                : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("# Attachments: "), Evidence.Media.IsSome
                ? UiIndirect(Evidence.Media.GetValueOrThrow().Count.ToString())
                : UiNoTranslate("n/a"),
            UiNewLines(1),
            Ui("Reported by: "), UiNoTranslate(ReportedBy.ByUser.FirstName));
    }
}