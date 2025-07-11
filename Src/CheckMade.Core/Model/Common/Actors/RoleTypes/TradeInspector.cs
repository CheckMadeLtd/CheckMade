using CheckMade.Core.Model.Common.Submissions;
using CheckMade.Core.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Common.Actors.RoleTypes;

public sealed record TradeInspector<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.None;
}