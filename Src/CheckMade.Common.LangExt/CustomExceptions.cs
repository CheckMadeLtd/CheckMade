namespace CheckMade.Common.LangExt;

public class DataAccessException(string? message = null, Exception? innerException = null) 
    : Exception(message, innerException);
public class NetworkAccessException(string? message = null, Exception? innerException = null) 
    : Exception(message, innerException);
public class DataMigrationException(string? message = null, Exception? innerException = null) 
    : Exception(message, innerException);
public class MonadicWrapperGetValueOrThrowException(string? message = null, Exception? innerException = null) 
    : Exception(message, innerException);