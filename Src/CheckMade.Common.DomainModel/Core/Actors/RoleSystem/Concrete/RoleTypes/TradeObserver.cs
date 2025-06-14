using CheckMade.Common.DomainModel.Core.Submissions.Concrete;
using CheckMade.Common.DomainModel.Core.Trades;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Core.Actors.RoleSystem.Concrete.RoleTypes;

public sealed record TradeObserver<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.All;
}