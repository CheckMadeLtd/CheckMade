using CheckMade.Abstract.Domain.Data.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Tests.Unit.Common;

public sealed class DomainTermTests
{
    [Fact]
    public void Equals_ReturnsFalse_ForTwoEnumsOfSameValueButDifferentType()
    {
        var enum1 = Dt(LanguageCode.en);
        var enum2 = Dt(ConsumablesItem.ToiletPaper);

        Assert.Equal((int)LanguageCode.en, (int)ConsumablesItem.ToiletPaper);
        Assert.False(enum1.Equals(enum2));
    }
}