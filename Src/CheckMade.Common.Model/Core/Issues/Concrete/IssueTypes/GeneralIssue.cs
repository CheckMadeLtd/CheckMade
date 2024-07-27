using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record GeneralIssue<T>(
        Guid Id, 
        DateTimeOffset CreationDate, 
        ISphereOfAction Sphere, 
        IssueEvidence Evidence, 
        IRoleInfo ReportedBy, 
        Option<IRoleInfo> HandledBy, 
        IssueStatus Status,
        IDomainGlossary Glossary) 
    : ITradeIssue<T>, IIssueWithEvidence where T : ITrade
{
    public UiString FormatDetails()
    {
        throw new NotImplementedException();
    }
}