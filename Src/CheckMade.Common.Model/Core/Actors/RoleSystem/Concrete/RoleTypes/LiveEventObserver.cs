using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public sealed record LiveEventObserver : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public IssueSummaryCategories GetIssueSummaryCategoriesForNotifications() =>
        IssueSummaryCategories.CommonBasics;
}