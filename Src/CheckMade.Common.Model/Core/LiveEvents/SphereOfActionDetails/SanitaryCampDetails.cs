using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Core.LiveEvents.SphereOfActionDetails;

public record SanitaryCampDetails(Option<Geo> Location) : ISphereOfActionDetails;