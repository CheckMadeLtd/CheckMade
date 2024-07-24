using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.Common.Model.Utils;

// ToDo: Review if actually needed, otherwise delete again. 

public static class SphereDetailsTypeByTrade
{
    public static Dictionary<Type, Type> Map { get; } = new()
    {
        { typeof(SaniCleanTrade), typeof(SaniCampDetails) },
        { typeof(SiteCleanTrade), typeof(SiteCleaningZoneDetails) }
    };
};