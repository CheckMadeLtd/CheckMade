using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.GIS;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ISphereOfActionDetails
{
    Option<Geo> GeoCoordinates { get; }
    IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
    Option<string> LocationName { get; }
}