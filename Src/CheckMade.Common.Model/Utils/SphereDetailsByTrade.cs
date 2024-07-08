using CheckMade.Common.Model.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Trades.Types;

namespace CheckMade.Common.Model.Utils;

// ToDo: Review if actually needed, otherwise delete again. 

public static class SphereDetailsTypeByTrade
{
    public static Dictionary<Type, Type> Map { get; } = new()
    {
        { typeof(TradeSaniClean), typeof(SanitaryCampDetails) },
        { typeof(TradeSiteCleaning), typeof(SiteCleaningZoneDetails) }
    };
};