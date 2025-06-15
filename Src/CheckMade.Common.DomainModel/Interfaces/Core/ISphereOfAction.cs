using CheckMade.Common.DomainModel.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Core;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}