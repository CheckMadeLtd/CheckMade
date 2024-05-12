using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IMessageHandler
{
    Task HandleMessageAsync(Message telegramInputMessage, BotType botType);
}

public class MessageHandler(
        IBotClientFactory botClientFactory,
        IRequestProcessorSelector selector,
        IToModelConverterFactory toModelConverterFactory,
        INetworkRetryPolicy retryPolicy,
        ILogger<MessageHandler> logger)
    : IMessageHandler
{
    internal const string CallToActionMessageAfterErrorReport = "Please report to your supervisor or contact support.";
    internal const string DataAccessExceptionErrorMessageStub = "An error has occured during a data access operation.";
    
    private BotType _botType;
    
    public async Task HandleMessageAsync(Message telegramInputMessage, BotType botType)
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
            logger.LogWarning("Received message of type '{messageType}': {warningMessage}", 
                telegramInputMessage.Type, BotUpdateSwitch.NoSpecialHandlingWarningMessage);
            return;
        }
        
        var inputMessage = await TryConvertToModelAsync(telegramInputMessage, toModelConverter);
        var outputMessage = await TryProcessInputIntoOutput(inputMessage);
        await TrySendOutput(outputMessage, botClient, chatId);
    }

    private async Task<InputMessage> TryConvertToModelAsync(
        Message telegramInputMessage, IToModelConverter toModelConverter)
    {
        try
        {
            return await toModelConverter.ConvertMessageAsync(telegramInputMessage);
        }
        catch (Exception ex)
        {
            throw new ToModelConversionException("Failed to convert Telegram Message to Model", ex);
        }
    }

    private async Task<string> TryProcessInputIntoOutput(InputMessage inputMessage)
    {
        var requestProcessor = selector.GetRequestProcessor(_botType);
        
        try
        {
            return await requestProcessor.EchoAsync(inputMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = ex switch
            { 
                DataAccessException => DataAccessExceptionErrorMessageStub,
                _ => $"A general error (type: {ex.GetType()}) has occured."
            };
            
            logger.LogError(ex, "{errMsg} Next, some details for debugging. " +
                                "BotType: {botType}; UserId: {userId}; " +
                                "DateTime of Input Message: {telegramDate}; " +
                                "Text of InputMessage: {text}", 
                errorMessage, _botType, inputMessage.UserId, 
                inputMessage.Details.TelegramDate, inputMessage.Details.Text);
            
            return $"{errorMessage} {CallToActionMessageAfterErrorReport}";
        }
    }

    private async Task TrySendOutput(string outputMessage, IBotClientWrapper botClient, ChatId chatId)
    {
        /* Telegram Servers have queues and handle retrying for sending from itself to end user, but this doesn't
        catch earlier network issues like from our Azure Function to the Telegram Servers! */
        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: outputMessage);
            });
        }
        catch (Exception ex)
        {
            throw new NetworkAccessException("Failed to reach Telegram servers.", ex);
        }
    }
}