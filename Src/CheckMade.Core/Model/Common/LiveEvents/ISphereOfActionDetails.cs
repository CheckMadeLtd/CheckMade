using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.GIS;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Common.LiveEvents;

public interface ISphereOfActionDetails
{
    Option<Geo> GeoCoordinates { get; }
    IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
    Option<string> LocationName { get; }
}