using CheckMade.Abstract.Domain.Model.Common.Submissions;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Common.Actors.RoleTypes;

public sealed record LiveEventObserver : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.CommonBasics;
}