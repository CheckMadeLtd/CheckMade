using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Common.Trades;

public interface ITrade
{
    UiString GetSphereOfActionLabel { get; }
}