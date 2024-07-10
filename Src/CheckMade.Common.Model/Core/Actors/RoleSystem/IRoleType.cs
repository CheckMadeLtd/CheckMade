using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem;

public interface IRoleType
{
    Option<ITrade> GetTrade();
}