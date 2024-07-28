namespace CheckMade.Common.Model.Core.Trades.Concrete;

public sealed record SaniCleanTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("Sanitary Camp");
}