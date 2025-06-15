using CheckMade.Common.DomainModel.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Data.Core;

public interface ISphereOfAction
{
    string Name { get; }
    ISphereOfActionDetails Details { get; }
    DbRecordStatus Status { get; }
    
    ITrade GetTradeInstance();
    Type GetTradeType();
}