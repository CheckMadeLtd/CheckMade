using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ITrade
{
    UiString GetSphereOfActionLabel { get; }
}