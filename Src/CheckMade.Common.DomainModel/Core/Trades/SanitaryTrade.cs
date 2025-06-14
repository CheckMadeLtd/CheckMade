using CheckMade.Common.DomainModel.Interfaces.Core;

namespace CheckMade.Common.DomainModel.Core.Trades;

public sealed record SanitaryTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Sanitary Camp</b>");
}