using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Data.Core.LiveEvents;

public sealed record SphereOfAction<T>(
    string Name,
    ISphereOfActionDetails Details,
    DbRecordStatus Status = DbRecordStatus.Active) 
    : ISphereOfAction where T : ITrade, new()
{
    public ITrade GetTradeInstance() => new T();
    public Type GetTradeType() => typeof(T);

    public bool Equals(SphereOfAction<T>? other)
    {
        return other is not null &&
               Name.Equals(other.Name) &&
               Details.GeoCoordinates == other.Details.GeoCoordinates &&
               Status.Equals(other.Status);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Name,
            Details.GeoCoordinates,
            Status);
    }
}