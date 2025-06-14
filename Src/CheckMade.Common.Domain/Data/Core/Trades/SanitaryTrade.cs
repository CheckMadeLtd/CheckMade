using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Data.Core.Trades;

public sealed record SanitaryTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Sanitary Camp</b>");
}