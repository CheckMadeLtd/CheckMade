using CheckMade.Abstract.Domain.Data.Core.Submissions;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}