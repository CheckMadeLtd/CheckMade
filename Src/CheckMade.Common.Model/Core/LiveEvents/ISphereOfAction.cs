using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.LiveEvents;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}