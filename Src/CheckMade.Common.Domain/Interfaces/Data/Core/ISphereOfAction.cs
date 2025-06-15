using CheckMade.Common.Domain.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}