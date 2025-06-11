using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core.Submissions.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem;

public interface IRoleType
{
    Option<ITrade> GetTradeInstance();
    Option<Type> GetTradeType();
    IssueSummaryCategories GetIssueSummaryCategoriesForNotifications();
}