using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record StaffIssue<T>(
        Guid Id,
        DateTimeOffset CreationDate, 
        ISphereOfAction Sphere, 
        IssueEvidence Evidence, 
        Role ReportedBy, 
        Option<Role> HandledBy, 
        IssueStatus Status,
        IDomainGlossary Glossary) 
    : ITradeIssue<T>, ITradeIssueWithEvidence<T> where T : ITrade
{
    public IReadOnlyDictionary<IssueSummaryCategories, UiString> GetSummary()
    {
        throw new NotImplementedException();
    }
}