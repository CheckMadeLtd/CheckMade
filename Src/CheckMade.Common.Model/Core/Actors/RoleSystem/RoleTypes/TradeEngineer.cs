using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.RoleTypes;

public class TradeEngineer<T> : ITradeRoleType<T> where T : ITrade;