using CheckMade.Common.Domain.Data.Core.Submissions;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.Core.Actors.RoleSystem.RoleTypes;

public sealed record TradeEngineer<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.AllExceptOperationalInfo;
}