namespace CheckMade.Common.Model.Core.Trades.Concrete.Types;

public class SiteCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 100;
    
    public bool DividesLiveEventIntoSpheresOfAction => true;
    public UiString GetSphereOfActionLabel => Ui("zone");
}