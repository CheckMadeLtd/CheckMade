namespace CheckMade.Common.Utils;

public class DataAccessException(string message, Exception innerException) : Exception(message, innerException);
public class NetworkAccessException(string message, Exception innerException) : Exception(message, innerException);
public class ToModelConversionException(string message, Exception innerException) : Exception(message, innerException);