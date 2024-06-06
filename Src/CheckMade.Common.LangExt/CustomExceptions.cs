namespace CheckMade.Common.LangExt;

public class TelegramBotClientCallException(string? message = null, Exception? innerException = null) 
    : Exception(message, innerException);

