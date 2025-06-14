using CheckMade.Common.DomainModel.Core.Submissions;
using CheckMade.Common.DomainModel.Interfaces.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Core.Actors.RoleSystem.RoleTypes;

public sealed record TradeObserver<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.All;
}