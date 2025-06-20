using CheckMade.Abstract.Domain.Model.Core.Submissions;
using CheckMade.Abstract.Domain.Model.Core.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Core.Actors.RoleTypes;

public sealed record LiveEventObserver : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.CommonBasics;
}