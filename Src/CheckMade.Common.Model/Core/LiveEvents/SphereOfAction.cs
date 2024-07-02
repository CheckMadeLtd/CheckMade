using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.LiveEvents;

public record SphereOfAction<T>(
        string Name,
        SphereOfActionDetails Details) 
    : ISphereOfAction where T : ITradeType
{
    public Type TradeType => typeof(T);
}