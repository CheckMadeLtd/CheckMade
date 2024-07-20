using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains;

namespace CheckMade.Common.Model.Core.Trades;

public interface ITradeIssue<T> : IIssue where T : ITrade
{
    Option<ITradeFacility<T>> Facility { get; }
}
