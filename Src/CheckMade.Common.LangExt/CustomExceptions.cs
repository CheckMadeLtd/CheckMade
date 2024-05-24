namespace CheckMade.Common.LangExt;

public class DataAccessException(string message, Exception? innerException = null) 
    : Exception(message, innerException);
public class NetworkAccessException(string message, Exception? innerException = null) 
    : Exception(message, innerException);
public class ToModelConversionException(string message, Exception? innerException = null) 
    : Exception(message, innerException);
public class DataMigrationException(string message, Exception? innerException = null) 
    : Exception(message, innerException);
public class MonadicWrapperGetValueOrThrowException(string? message, Exception? innerException = null) 
    : Exception(message, innerException);