using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.LiveEvents.Concrete;

public sealed record SphereOfAction<T>(
        string Name,
        ISphereOfActionDetails Details,
        DbRecordStatus Status = DbRecordStatus.Active) 
    : ISphereOfAction where T : ITrade, new()
{
    public ITrade GetTradeInstance() => new T();
    public Type GetTradeType() => typeof(T);
}