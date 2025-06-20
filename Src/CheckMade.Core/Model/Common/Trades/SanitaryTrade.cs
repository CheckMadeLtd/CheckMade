using General.Utils.UiTranslation;

namespace CheckMade.Core.Model.Common.Trades;

public sealed record SanitaryTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Sanitary Camp</b>");
}