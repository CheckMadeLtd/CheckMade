using CheckMade.Common.DomainModel.Data.Core.Submissions;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem.RoleTypes;

public sealed record LiveEventObserver : IRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();

    public SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications() =>
        SubmissionSummaryCategories.CommonBasics;
}