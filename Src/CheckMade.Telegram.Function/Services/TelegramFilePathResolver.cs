namespace CheckMade.Telegram.Function.Services;

public interface ITelegramFilePathResolver
{
    Task<string> GetTelegramFilePathAsync(string fileId);
}

internal class TelegramFilePathResolver(IBotClientWrapper botClient) : ITelegramFilePathResolver
{
    internal const string TelegramBotDownloadFileApiUrlStub = "https://api.telegram.org/file/";
    
    public async Task<string> GetTelegramFilePathAsync(string fileId)
    {
        var file = await botClient.GetFileAsync(fileId);
        return TelegramBotDownloadFileApiUrlStub + $"bot{botClient.BotToken}/{file.FilePath}";  
    }
}
