using CheckMade.Abstract.Domain.Data.Core.Submissions;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem.RoleTypes;

public sealed record LiveEventAdmin : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.All;
}