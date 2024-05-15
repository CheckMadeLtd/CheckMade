using CheckMade.Common.LanguageExtensions;
using CheckMade.Common.LanguageExtensions.MonadicWrappers;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IMessageHandler
{
    Task<Attempt<Unit>> HandleMessageAsync(Message telegramInputMessage, BotType botType);
}

public class MessageHandler(
        IBotClientFactory botClientFactory,
        IRequestProcessorSelector selector,
        IToModelConverterFactory toModelConverterFactory,
        ILogger<MessageHandler> logger)
    : IMessageHandler
{
    internal const string CallToActionMessageAfterErrorReport = "Please report to your supervisor or contact support.";
    
    private BotType _botType;
    
    public async Task<Attempt<Unit>> HandleMessageAsync(Message telegramInputMessage, BotType botType)
    {
        ChatId chatId = telegramInputMessage.Chat.Id;
        _botType = botType;
        var botClient = botClientFactory.CreateBotClient(_botType);
        var filePathResolver = new TelegramFilePathResolver(botClient);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver);
        
        logger.LogInformation("Invoked telegram update function for BotType: {botType} " +
                              "with Message from UserId/ChatId: {userId}/{chatId}", 
            _botType, telegramInputMessage.From?.Id ?? 0, chatId);

        var handledMessageTypes = new[]
        {
            MessageType.Audio,
            MessageType.Document,
            MessageType.Photo,
            MessageType.Text,
            MessageType.Video,
            MessageType.Voice
        };

        if (!handledMessageTypes.Contains(telegramInputMessage.Type))
        {
            const string warningMessage = "Received message of type '{0}': {1}";
            
            logger.LogWarning(string.Format(warningMessage, 
                telegramInputMessage.Type, BotUpdateSwitch.NoSpecialHandlingWarningMessage));

            var sendWarningOutcome = (await SendOutputAsync(
                    string.Format(warningMessage,
                        telegramInputMessage.Type, BotUpdateSwitch.NoSpecialHandlingWarningMessage), 
                    botClient,
                    chatId))
                .Match(
                    _ => Attempt<Unit>.Succeed(),
                    Attempt<Unit>.Fail);

            return sendWarningOutcome;
        }
        
        var modelInputMessage = await Attempt<InputMessage>.RunAsync(() => 
            toModelConverter.ConvertMessageOrThrowAsync(telegramInputMessage));
        
        var outputMessage = await modelInputMessage
            .Match(
                async inputMessage => 
                    await selector.GetRequestProcessor(_botType).SafelyEchoAsync(inputMessage),
                exception => throw exception
            );
        
        var sendOutputOutcome = await outputMessage.Match(
            
            async message => await SendOutputAsync(message, botClient, chatId),
            
            async ex =>
            {
                logger.LogError(ex, "{errMsg} Next, some details for debugging. " +
                                    "BotType: {botType}; Telegram user Id: {userId}; " +
                                    "DateTime of received Message: {telegramDate}; " +
                                    "with text: {text}", 
                    ex.Message, _botType, telegramInputMessage.From!.Id, 
                    telegramInputMessage.Date, telegramInputMessage.Text);

                await SendOutputAsync($"{ex.Message} {CallToActionMessageAfterErrorReport}", botClient, chatId);
                
                return Attempt<Unit>.Fail(ex);
            });
        
        return sendOutputOutcome;
    }

    private async Task<Attempt<Unit>> SendOutputAsync(string outputMessage, IBotClientWrapper botClient, ChatId chatId)
    {
        return await Attempt<Unit>.RunAsync(async () =>
            await botClient.SendTextMessageAsync(chatId, outputMessage));
    }
}