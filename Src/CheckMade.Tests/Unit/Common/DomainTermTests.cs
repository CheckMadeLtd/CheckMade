using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;

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