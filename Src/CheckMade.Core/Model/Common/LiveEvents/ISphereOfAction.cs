using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.Trades;

namespace CheckMade.Core.Model.Common.LiveEvents;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}