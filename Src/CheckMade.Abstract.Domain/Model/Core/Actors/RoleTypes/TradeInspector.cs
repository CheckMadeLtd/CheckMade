using CheckMade.Abstract.Domain.Model.Core.Submissions;
using CheckMade.Abstract.Domain.Model.Core.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Core.Actors.RoleTypes;

public sealed record TradeInspector<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.None;
}