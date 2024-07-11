namespace CheckMade.Common.Model.Core.Trades.Concrete.Types;

public class SaniCleanTrade : ITrade
{
    public bool DividesLiveEventIntoSpheresOfAction => true;
    public const int SphereNearnessThresholdInMeters = 30;
}