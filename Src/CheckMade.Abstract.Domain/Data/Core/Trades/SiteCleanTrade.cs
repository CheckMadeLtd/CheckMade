using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Data.Core.Trades;

public sealed record SiteCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 100;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Zone</b>");
}