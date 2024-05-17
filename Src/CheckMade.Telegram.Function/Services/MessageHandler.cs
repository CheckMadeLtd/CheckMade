using CheckMade.Common.FpExt;
using CheckMade.Common.FpExt.MonadicWrappers;
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
            logger.LogWarning("Received message of type '{0}': {1}", 
                telegramInputMessage.Type, BotUpdateSwitch.NoSpecialHandlingWarningMessage);

            return Attempt<Unit>.Succeed(Unit.Value);
        }

        var botClient = 
            Attempt<IBotClientWrapper>.Run(() => 
                botClientFactory.CreateBotClientOrThrow(_botType))
                .Match(
                    botClient => botClient,
                    ex => throw new Exception("Failed to create BotClient", ex));

        var filePathResolver = new TelegramFilePathResolver(botClient);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver);
        
        var sendOutputOutcome =
            from modelInputMessage in Attempt<InputMessage>.RunAsync(() => 
                toModelConverter.ConvertMessageOrThrowAsync(telegramInputMessage))
            from outputMessage in selector.GetRequestProcessor(_botType).SafelyEchoAsync(modelInputMessage)
            select SendOutputAsync(outputMessage, botClient, chatId);        
        
        return (await sendOutputOutcome).Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            ex =>
            {
                logger.LogError(ex, "{errMsg} Next, some details for debugging. " +
                                    "BotType: {botType}; Telegram user Id: {userId}; " +
                                    "DateTime of received Message: {telegramDate}; " +
                                    "with text: {text}",
                    ex.Message, _botType, telegramInputMessage.From!.Id,
                    telegramInputMessage.Date, telegramInputMessage.Text);

                // fire and forget
                _ = SendOutputAsync($"{ex.Message} {CallToActionMessageAfterErrorReport}", botClient, chatId);
                return Attempt<Unit>.Fail(ex);
            });
    }

    private async Task<Attempt<Unit>> SendOutputAsync(string outputMessage, IBotClientWrapper botClient, ChatId chatId)
    {
        return await Attempt<Unit>.RunAsync(async () =>
            await botClient.SendTextMessageAsync(chatId, outputMessage));
    }
}