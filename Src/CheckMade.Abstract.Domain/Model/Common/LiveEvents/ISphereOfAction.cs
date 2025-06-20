using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using CheckMade.Abstract.Domain.Model.Common.Trades;

namespace CheckMade.Abstract.Domain.Model.Common.LiveEvents;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}