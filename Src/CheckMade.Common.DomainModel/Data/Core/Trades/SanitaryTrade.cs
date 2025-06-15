using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Data.Core.Trades;

public sealed record SanitaryTrade : ITrade
{
    public const int SphereNearnessThresholdInMeters = 30;
    
    public UiString GetSphereOfActionLabel => Ui("<b>Sanitary Camp</b>");
}