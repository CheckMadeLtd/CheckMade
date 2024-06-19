using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.SanitaryOps.Issues;

namespace CheckMade.Tests.Fine.Unit.Common;

public class DomainTermTests
{
    [Fact]
    public void Equals_ReturnsFalse_ForTwoEnumsOfSameValueButDifferentType()
    {
        var enum1 = Dt(LanguageCode.en);
        var enum2 = Dt(ConsumablesIssue.Item.ToiletPaper);
        
        //Assert.Equal(enum1.Value, enum2.Value);
        Assert.False(enum1.Equals(enum2));
    }
}