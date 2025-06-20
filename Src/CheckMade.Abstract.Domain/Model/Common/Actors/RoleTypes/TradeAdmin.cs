using CheckMade.Abstract.Domain.Model.Common.Submissions;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Common.Actors.RoleTypes;

public sealed record TradeAdmin<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.All;
}