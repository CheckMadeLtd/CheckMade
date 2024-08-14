namespace CheckMade.Common.Model.Core.Trades.Concrete;

public sealed record SanitaryTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 20;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Sanitary Camp</b>");
}