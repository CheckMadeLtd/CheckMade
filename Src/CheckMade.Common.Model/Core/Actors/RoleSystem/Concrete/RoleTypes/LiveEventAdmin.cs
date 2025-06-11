using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core.Submissions.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public sealed record LiveEventAdmin : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public IssueSummaryCategories GetIssueSummaryCategoriesForNotifications() =>
        IssueSummaryCategories.All;
}