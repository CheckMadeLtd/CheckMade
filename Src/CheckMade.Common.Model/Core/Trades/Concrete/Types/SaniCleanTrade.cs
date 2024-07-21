namespace CheckMade.Common.Model.Core.Trades.Concrete.Types;

public class SaniCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("sanitary camp");
}