using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Trades.Types;

namespace CheckMade.Common.Model.Core.Trades.SubDomains.SanitaryOps.Issues;

public record CleanlinessIssue(
        DateTime CreationDate,
        ISphereOfAction Sphere,
        Geo Location,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        ITradeFacility<TradeSanitaryOps> Facility,
        IssueStatus Status = IssueStatus.Reported) 
    : ITradeIssue<TradeSanitaryOps>;