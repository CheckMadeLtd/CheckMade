using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Startup;
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
        DefaultUiLanguageCodeProvider defaultUiLanguage,
        IUiTranslatorFactory translatorFactory,
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

        // Will retrieve actual user language preference
        var userLanguagePreference = Option<LanguageCode>.None();

        var currentUiLanguage = userLanguagePreference.Match(
            code => code,
            () => defaultUiLanguage.Code);
        
        var translator = translatorFactory.Create(currentUiLanguage);
        
        var filePathResolver = new TelegramFilePathResolver(botClient);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver, translator);
        
        var sendOutputOutcome =
            from modelInputMessage in Attempt<InputMessage>.RunAsync(async () => 
                await toModelConverter.ConvertMessageOrThrowAsync(telegramInputMessage, botType))
            from outputMessage in selector.GetRequestProcessor(botType).SafelyEchoAsync(modelInputMessage)
            select SendOutputAsync(outputMessage, botClient, chatId, translator);
        
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
                _ = SendOutputAsync(UiConcatenate(UiNoTranslate(ex.Message), CallToActionAfterErrorReport),
                    botClient, chatId, translator);
                return Attempt<Unit>.Fail(ex);
            });
    }

    private async Task<Attempt<Unit>> SendOutputAsync(
        UiString outputMessage, IBotClientWrapper botClient, ChatId chatId, IUiTranslator translator)
    {
        return await Attempt<Unit>.RunAsync(async () =>
            await botClient.SendTextMessageOrThrowAsync(chatId, translator.Translate(outputMessage)));
    }
}