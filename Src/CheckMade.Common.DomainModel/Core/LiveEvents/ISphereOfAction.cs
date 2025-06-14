using CheckMade.Common.DomainModel.Core.Trades;
using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.Common.DomainModel.Core.LiveEvents;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}