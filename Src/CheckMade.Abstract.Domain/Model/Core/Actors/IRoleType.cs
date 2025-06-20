using CheckMade.Abstract.Domain.Model.Core.Submissions;
using CheckMade.Abstract.Domain.Model.Core.Trades;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Core.Actors;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}