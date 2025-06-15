using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ITrade
{
    UiString GetSphereOfActionLabel { get; }
}