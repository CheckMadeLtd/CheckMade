using CheckMade.Common.Domain.Data.Core.GIS;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ISphereOfActionDetails
{
    Option<Geo> GeoCoordinates { get; }
    IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
    Option<string> LocationName { get; }
}