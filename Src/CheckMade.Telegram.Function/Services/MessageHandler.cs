using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
    private static readonly UiString CallToActionAfterErrorReport =
        Ui("Please contact technical support or your supervisor.");

    private static IUiTranslator? _uiTranslator;
    
    public async Task<Attempt<Unit>> SafelyHandleMessageAsync(Message telegramInputMessage, BotType botType)
    {
        _uiTranslator = translatorFactory.Create(GetUiLanguage(telegramInputMessage));
        
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
                telegramInputMessage.Type, BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish());

            return Unit.Value;
        }

        var botClient = 
            Attempt<IBotClientWrapper>.Run(() => 
                botClientFactory.CreateBotClientOrThrow(botType))
                .Match(
                    botClient => botClient,
                    failure => throw new InvalidOperationException(
                        "Failed to create BotClient", failure.Exception));

        var filePathResolver = new TelegramFilePathResolver(botClient);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver);
        
        var sendOutputOutcome =
            from modelInputMessage in await toModelConverter.SafelyConvertMessageAsync(telegramInputMessage, botType)
            from outputMessage in selector.GetRequestProcessor(botType).SafelyEchoAsync(modelInputMessage)
            select SendOutputAsync(outputMessage, botClient, chatId);
        
        return (await sendOutputOutcome).Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            failure =>
            {
                if (failure.Error != null)
                {
                    logger.LogError($"Message from Attempt<T>.Failure.Error: " +
                                    $"{failure.Error.GetFormattedEnglish()}");
                }
                
                logger.LogError(failure.Exception, 
                    "Next, some details to help debug the current error. " +
                                    "BotType: '{botType}'; Telegram user Id: '{userId}'; " +
                                    "DateTime of received Message: '{telegramDate}'; with text: '{text}'",
                    botType, telegramInputMessage.From!.Id,
                    telegramInputMessage.Date, telegramInputMessage.Text);

                var errorMessageToUser = failure.Exception != null 
                    ? UiNoTranslate(failure.Exception.Message) 
                    : failure.Error;
                
                // fire and forget
                _ = SendOutputAsync(UiConcatenate(
                        errorMessageToUser ?? UiNoTranslate("No error message"),
                        UiNoTranslate(" "),
                        CallToActionAfterErrorReport), botClient, chatId)
                    // this ensures logging of any NetworkAccessException thrown by SendTextMessageOrThrowAsync
                    .ContinueWith(task => 
                    { 
                        if (task.Result.IsFailure) 
                            logger.LogError(
                                "An error occurred while trying to send a message to report another error."); 
                    });
                
                return failure;
            });
    }

    // FYI: There is a time delay of a couple of minutes on Telegram side when user switches lang. setting in Tlgr client
    private LanguageCode GetUiLanguage(Message telegramInputMessage)
    {
        var userLanguagePreferenceIsRecognized = Enum.TryParse(
            typeof(LanguageCode),
            telegramInputMessage.From?.LanguageCode,
            true,
            out var userLanguagePreference);
        
        return userLanguagePreferenceIsRecognized
            ? (LanguageCode) userLanguagePreference!
            : defaultUiLanguage.Code;
    }
    
    private static async Task<Attempt<Unit>> SendOutputAsync(
        UiString outputMessage, IBotClientWrapper botClient, ChatId chatId)
    {
        return await Attempt<Unit>.RunAsync(async () =>
            await botClient.SendTextMessageOrThrowAsync(
                chatId, 
                _uiTranslator?.Translate(outputMessage) 
                ?? throw new ArgumentNullException(nameof(outputMessage), 
                    "UiTranslator or translated OutputMessage must not be NULL."), 
                Option<IReplyMarkup>.None())); // ToDo: replace with ReplyMarkup to be contained in outputMessageDto
    }
}