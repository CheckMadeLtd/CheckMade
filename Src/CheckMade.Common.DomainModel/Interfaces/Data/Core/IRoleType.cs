using CheckMade.Common.DomainModel.Data.Core.Submissions;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Interfaces.Data.Core;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}