using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Trades.Types;

namespace CheckMade.Common.Model.Core.Trades.SubDomains.SaniClean.Issues;

public record CleanlinessIssue(
        DateTime CreationDate,
        ISphereOfAction Sphere,
        Option<ITradeFacility<SaniCleanTrade>> Facility,
        Geo Location,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status = IssueStatus.Reported) 
    : ITradeIssue<SaniCleanTrade>;