using CheckMade.Common.DomainModel.Interfaces.Core;

namespace CheckMade.Common.DomainModel.Core.Trades;

public sealed record SiteCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 100;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Zone</b>");
}