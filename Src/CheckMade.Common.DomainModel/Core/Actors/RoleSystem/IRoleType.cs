using CheckMade.Common.DomainModel.Core.Submissions.Concrete;
using CheckMade.Common.DomainModel.Core.Trades;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Core.Actors.RoleSystem;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    SubmissionSummaryCategories GetSubmissionSummaryCategoriesForNotifications();
}