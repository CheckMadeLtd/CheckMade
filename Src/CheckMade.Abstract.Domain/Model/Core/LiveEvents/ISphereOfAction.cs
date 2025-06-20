using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using CheckMade.Abstract.Domain.Model.Core.Trades;

namespace CheckMade.Abstract.Domain.Model.Core.LiveEvents;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}