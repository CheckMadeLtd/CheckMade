using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Core.Trades;

public sealed record SiteCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 100;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Zone</b>");
}