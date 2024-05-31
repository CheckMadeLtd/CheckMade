using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.DTOs;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services.UpdateHandling;

public interface IMessageHandler
{
    Task<Attempt<Unit>> HandleMessageAsync(UpdateWrapper update, BotType botType);
}

public class MessageHandler(
        IBotClientFactory botClientFactory,
        IRequestProcessorSelector selector,
        IToModelConverterFactory toModelConverterFactory,
        DefaultUiLanguageCodeProvider defaultUiLanguage,
        IUiTranslatorFactory translatorFactory,
        IOutputToReplyMarkupConverterFactory replyMarkupConverterFactory,
        ILogger<MessageHandler> logger)
    : IMessageHandler
{
    private static readonly UiString CallToActionAfterErrorReport =
        Ui("Please contact technical support or your supervisor.");

    private IUiTranslator? _uiTranslator;
    private IOutputToReplyMarkupConverter? _replyMarkupConverter;

    public async Task<Attempt<Unit>> HandleMessageAsync(UpdateWrapper update, BotType botType)
    {
        ChatId chatId = update.Message.Chat.Id;
        
        _uiTranslator = translatorFactory.Create(GetUiLanguage(update.Message));
        _replyMarkupConverter = replyMarkupConverterFactory.Create(_uiTranslator);
        
        logger.LogInformation("Invoked telegram update function for BotType: {botType} " +
                              "with Message from UserId/ChatId: {userId}/{chatId}", 
            botType, update.Message.From?.Id ?? 0, chatId);

        var handledMessageTypes = new[]
        {
            MessageType.Audio,
            MessageType.Document,
            MessageType.Location,
            MessageType.Photo,
            MessageType.Text
        };

        if (!handledMessageTypes.Contains(update.Message.Type))
        {
            logger.LogWarning("Received message of type '{messageType}': {warning}", 
                update.Message.Type, BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish());

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
            from modelInputMessage in await toModelConverter.ConvertToModelAsync(update, botType)
            from output in selector.GetRequestProcessor(botType).ProcessRequestAsync(modelInputMessage)
            select SendOutputAsync(output, botClient, chatId);
        
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
                    botType, update.Message.From!.Id,
                    update.Message.Date, update.Message.Text);

                var errorOutput = OutputDto.Create(
                    UiConcatenate(
                        UiNoTranslate(failure.Exception?.Message ?? string.Empty), 
                        failure.Error,
                        UiNoTranslate(" "),
                        CallToActionAfterErrorReport));
                
                _ = SendOutputAsync(errorOutput, botClient, chatId)
                    .ContinueWith(task => 
                    { 
                        if (task.Result.IsFailure) // e.g. NetworkAccessException thrown downstream 
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
    
    private async Task<Attempt<Unit>> SendOutputAsync(
        OutputDto output, IBotClientWrapper botClient, ChatId chatId)
    {
        if (_uiTranslator == null)
            throw new InvalidOperationException("UiTranslator or translated OutputMessage must not be NULL.");

        if (_replyMarkupConverter == null)
            throw new InvalidOperationException("ReplyMarkupConverter must not be null.");
        
        return await Attempt<Unit>.RunAsync(async () =>
            await botClient.SendTextMessageOrThrowAsync(
                chatId, 
                _uiTranslator.Translate(Ui("𓃑 Please choose:")),
                _uiTranslator.Translate(output.Text.GetValueOrDefault()),
                _replyMarkupConverter.GetReplyMarkup(output))
            );
    }
}