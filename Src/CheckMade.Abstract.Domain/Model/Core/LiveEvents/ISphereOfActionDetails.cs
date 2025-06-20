using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using CheckMade.Abstract.Domain.Model.Core.GIS;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Core.LiveEvents;

public interface ISphereOfActionDetails
{
    Option<Geo> GeoCoordinates { get; }
    IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
    Option<string> LocationName { get; }
}