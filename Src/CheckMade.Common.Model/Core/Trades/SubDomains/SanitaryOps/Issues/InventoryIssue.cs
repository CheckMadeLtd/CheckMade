using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Trades.Types;

namespace CheckMade.Common.Model.Core.Trades.SubDomains.SanitaryOps.Issues;

public record InventoryIssue(
        DateTime CreationDate,
        ISphereOfAction Sphere,
        Option<ITradeFacility<TradeSanitaryOps>> Facility,
        Geo Location,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status) 
    : ITradeIssue<TradeSanitaryOps>;