using CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    IssueSummaryCategories GetIssueSummaryCategoriesForNotifications();
    AssessmentSummaryCategories GetAssessmentSummaryCategoriesForNotifications();
}