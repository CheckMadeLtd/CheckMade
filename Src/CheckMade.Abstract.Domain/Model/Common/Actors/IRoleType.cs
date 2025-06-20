using CheckMade.Abstract.Domain.Model.Common.Submissions;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Common.Actors;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}