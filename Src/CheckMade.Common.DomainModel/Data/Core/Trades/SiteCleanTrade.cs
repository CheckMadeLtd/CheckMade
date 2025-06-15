using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Data.Core.Trades;

public sealed record SiteCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 100;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Zone</b>");
}