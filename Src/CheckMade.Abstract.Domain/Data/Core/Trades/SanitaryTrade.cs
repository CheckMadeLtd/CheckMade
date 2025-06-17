using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Data.Core.Trades;

public sealed record SanitaryTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Sanitary Camp</b>");
}