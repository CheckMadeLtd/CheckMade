using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;

namespace CheckMade.Tests.Unit.Common;

public class DomainTermTests
{
    [Fact]
    public void Equals_ReturnsFalse_ForTwoEnumsOfSameValueButDifferentType()
    {
        var enum1 = Dt(LanguageCode.en);
        var enum2 = Dt(SaniConsumables.Item.ToiletPaper);

        Assert.Equal((int)LanguageCode.en, (int)SaniConsumables.Item.ToiletPaper);
        Assert.False(enum1.Equals(enum2));
    }
}