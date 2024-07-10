using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem;

public interface ITradeRoleType<T> : IRoleType where T : ITrade, new();