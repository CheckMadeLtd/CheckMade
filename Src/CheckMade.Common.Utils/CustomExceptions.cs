namespace CheckMade.Common.Utils;

public class DataAccessException(string message, Exception innerException) : Exception(message, innerException);