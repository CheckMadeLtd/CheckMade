using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.LiveEvents;

public record SphereOfAction<T>(
        string Name,
        SphereOfActionDetails Details,
        DbRecordStatus Status = DbRecordStatus.Active) 
    : ISphereOfAction where T : ITrade
{
    public Type Trade => typeof(T);
}