namespace CheckMade.Common.LangExt;

public class DataAccessException(UiString message, Exception? innerException = null) 
    : Exception(message.RawOriginalText, innerException);
public class NetworkAccessException(UiString message, Exception? innerException = null) 
    : Exception(message.RawOriginalText, innerException);
public class ToModelConversionException(UiString message, Exception? innerException = null) 
    : Exception(message.RawOriginalText, innerException);
public class DataMigrationException(UiString message, Exception? innerException = null) 
    : Exception(message.RawOriginalText, innerException);
public class MonadicWrapperGetValueOrThrowException(UiString? message, Exception? innerException = null) 
    : Exception(message?.RawOriginalText, innerException);