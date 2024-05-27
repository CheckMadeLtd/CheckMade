using CheckMade.Telegram.Function.Services.BotClient;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface ITelegramFilePathResolver
{
    Task<Attempt<string>> SafelyGetTelegramFilePathAsync(string fileId);
}

internal class TelegramFilePathResolver(IBotClientWrapper botClient) : ITelegramFilePathResolver
{
    internal const string TelegramBotDownloadFileApiUrlStub = "https://api.telegram.org/file/";
    
    public async Task<Attempt<string>> SafelyGetTelegramFilePathAsync(string fileId)
    {
        var fileAttempt = await Attempt<File>.RunAsync(async () => await botClient.GetFileOrThrowAsync(fileId));
        
        return fileAttempt.Match(
            file => TelegramBotDownloadFileApiUrlStub + $"bot{botClient.BotToken}/{file.FilePath}",
            Attempt<string>.Fail);  
    }
}
