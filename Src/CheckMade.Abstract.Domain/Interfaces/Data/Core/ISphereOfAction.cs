using CheckMade.Abstract.Domain.Data.Core;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}