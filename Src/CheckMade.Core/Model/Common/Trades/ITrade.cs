using General.Utils.UiTranslation;

namespace CheckMade.Core.Model.Common.Trades;

public interface ITrade
{
    UiString GetSphereOfActionLabel { get; }
}