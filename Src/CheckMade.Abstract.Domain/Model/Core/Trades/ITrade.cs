using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Core.Trades;

public interface ITrade
{
    UiString GetSphereOfActionLabel { get; }
}