using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using CheckMade.Abstract.Domain.Model.Common.GIS;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Common.LiveEvents;

public interface ISphereOfActionDetails
{
    Option<Geo> GeoCoordinates { get; }
    IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
    Option<string> LocationName { get; }
}