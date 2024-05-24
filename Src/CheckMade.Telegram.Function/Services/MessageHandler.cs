using CheckMade.Common.LangExt;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IMessageHandler
{
    Task<Attempt<Unit>> SafelyHandleMessageAsync(Message telegramInputMessage, BotType botType);
}

public class MessageHandler(
        IBotClientFactory botClientFactory,
        IRequestProcessorSelector selector,
        IToModelConverterFactory toModelConverterFactory,
        IUiTranslator translator,
        ILogger<MessageHandler> logger)
    : IMessageHandler
{
    internal static readonly UiString CallToActionAfterErrorReport = 
        Ui("Bitte kontaktiere den Support oder deinen Supervisor.");
    
    public async Task<Attempt<Unit>> SafelyHandleMessageAsync(Message telegramInputMessage, BotType botType)
    {
        ChatId chatId = telegramInputMessage.Chat.Id;
        
        logger.LogInformation("Invoked telegram update function for BotType: {botType} " +
                              "with Message from UserId/ChatId: {userId}/{chatId}", 
            botType, telegramInputMessage.From?.Id ?? 0, chatId);

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
            logger.LogWarning("Received message of type '{messageType}': {warning}", 
                telegramInputMessage.Type, BotUpdateSwitch.NoSpecialHandlingWarning);

            return Attempt<Unit>.Succeed(Unit.Value);
        }

        var botClient = 
            Attempt<IBotClientWrapper>.Run(() => 
                botClientFactory.CreateBotClientOrThrow(botType))
                .Match(
                    botClient => botClient,
                    ex => throw new InvalidOperationException(
                        "Failed to create BotClient", ex));

        var filePathResolver = new TelegramFilePathResolver(botClient);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver, translator);
        
        var sendOutputOutcome =
            from modelInputMessage in Attempt<InputMessage>.RunAsync(async () => 
                await toModelConverter.ConvertMessageOrThrowAsync(telegramInputMessage, botType))
            from outputMessage in selector.GetRequestProcessor(botType).SafelyEchoAsync(modelInputMessage)
            select SendOutputAsync(outputMessage, botClient, chatId);        
        
        return (await sendOutputOutcome).Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            ex =>
            {
                logger.LogError(ex, "Next, some details for debugging the upcoming error log entry. " +
                                    "BotType: '{botType}'; Telegram user Id: '{userId}'; " +
                                    "DateTime of received Message: '{telegramDate}'; " +
                                    "with text: '{text}'",
                    botType, telegramInputMessage.From!.Id,
                    telegramInputMessage.Date, telegramInputMessage.Text);

                // fire and forget
                _ = SendOutputAsync(UiConcatenate(UiIndirect(ex.Message), CallToActionAfterErrorReport),
                    botClient, chatId);
                return Attempt<Unit>.Fail(ex);
            });
    }

    private async Task<Attempt<Unit>> SendOutputAsync(
        UiString outputMessage, IBotClientWrapper botClient, ChatId chatId)
    {
        return await Attempt<Unit>.RunAsync(async () =>
            await botClient.SendTextMessageOrThrowAsync(chatId, translator.Translate(outputMessage)));
    }
}