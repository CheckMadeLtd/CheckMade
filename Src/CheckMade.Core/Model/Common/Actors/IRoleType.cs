using CheckMade.Core.Model.Common.Submissions;
using CheckMade.Core.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Common.Actors;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}