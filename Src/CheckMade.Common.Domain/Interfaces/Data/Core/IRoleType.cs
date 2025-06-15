using CheckMade.Common.Domain.Data.Core.Submissions;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}