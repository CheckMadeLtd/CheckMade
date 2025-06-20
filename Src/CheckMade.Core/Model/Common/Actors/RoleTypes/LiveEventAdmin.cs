using CheckMade.Core.Model.Common.Submissions;
using CheckMade.Core.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Common.Actors.RoleTypes;

public sealed record LiveEventAdmin : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.All;
}