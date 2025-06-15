using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Data.Core.Trades;

public sealed record SiteCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 100;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Zone</b>");
}