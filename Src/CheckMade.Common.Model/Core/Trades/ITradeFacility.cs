namespace CheckMade.Common.Model.Core.Trades;

public interface ITradeFacility<T> : IFacility where T : ITrade;