using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Interfaces;

public interface ISphereOfAction
{
    string Name { get; }
    Type TradeType { get; }
    SphereOfActionDetails Details { get; }
}