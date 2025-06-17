using CheckMade.Abstract.Domain.Data.Core.Submissions;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem.RoleTypes;

public sealed record TradeInspector<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.None;
}