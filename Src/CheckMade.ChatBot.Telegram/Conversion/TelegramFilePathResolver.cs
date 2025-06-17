using CheckMade.ChatBot.Telegram.BotClient;
using General.Utils.FpExtensions.Monads;
using File = Telegram.Bot.Types.TGFile;

namespace CheckMade.ChatBot.Telegram.Conversion;

public interface ITelegramFilePathResolver
{
    Task<Result<string>> GetTelegramFilePathAsync(string fileId);
}

internal sealed class TelegramFilePathResolver(IBotClientWrapper botClient) : ITelegramFilePathResolver
{
    private const string TelegramBotDownloadFileApiUrlStub = "https://api.telegram.org/file/";
    
    public async Task<Result<string>> GetTelegramFilePathAsync(string fileId)
    {
        var fileAttempt = await Result<File>.RunAsync(async () => await botClient.GetFileAsync(fileId));
        
        return fileAttempt.Match(
            file => TelegramBotDownloadFileApiUrlStub + $"bot{botClient.MyBotToken}/{file.FilePath}",
            Result<string>.Fail);  
    }
}
