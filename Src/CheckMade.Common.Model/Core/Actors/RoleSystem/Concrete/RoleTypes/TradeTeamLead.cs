using CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public sealed record TradeTeamLead<T> : IRoleType where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);

    public IssueSummaryCategories GetIssueSummaryCategoriesForNotifications() =>
        IssueSummaryCategories.AllExceptOperationalInfo;

    public AssessmentSummaryCategories GetAssessmentSummaryCategoriesForNotifications() =>
        AssessmentSummaryCategories.AllExceptOperationalInfo;
}