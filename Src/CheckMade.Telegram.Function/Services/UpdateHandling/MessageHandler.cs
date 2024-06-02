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
    private IUiTranslator? _uiTranslator;
    private IOutputToReplyMarkupConverter? _replyMarkupConverter;

    public async Task<Attempt<Unit>> HandleMessageAsync(UpdateWrapper update, BotType botType)
    {
        ChatId chatId = update.Message.Chat.Id;
        
        _uiTranslator = translatorFactory.Create(GetUiLanguage(update.Message));
        _replyMarkupConverter = replyMarkupConverterFactory.Create(_uiTranslator);
        
        logger.LogTrace("Invoked telegram update function for BotType: {botType} " +
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

        var receivingBotClient = 
            Attempt<IBotClientWrapper>.Run(() => 
                botClientFactory.CreateBotClientOrThrow(botType))
                .Match(
                    botClient => botClient,
                    error => throw new InvalidOperationException(
                        "Failed to create BotClient", error.Exception));
        
        var filePathResolver = new TelegramFilePathResolver(receivingBotClient);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver);
        
        // ToDo: below, need to probably replace receivingBotClient with a botClientByTypeDictionary 
        var sendOutputsOutcome =
            from modelInputMessage in await toModelConverter.ConvertToModelAsync(update, botType)
            from outputs in selector.GetRequestProcessor(botType).ProcessRequestAsync(modelInputMessage)
            select SendOutputsAsync(outputs, receivingBotClient, chatId);
        
        return (await sendOutputsOutcome).Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            error =>
            {
                if (error.FailureMessage != null)
                {
                    logger.LogWarning($"Message to User from {nameof(error.FailureMessage)}: " +
                                    $"{error.FailureMessage.GetFormattedEnglish()}");
                    
                    _ = SendOutputsAsync(new List<OutputDto>{ OutputDto.Create(error.FailureMessage) }, 
                            receivingBotClient, chatId)
                        .ContinueWith(task => 
                        { 
                            if (task.Result.IsError) // e.g. NetworkAccessException thrown downstream 
                                logger.LogWarning($"An error occurred while trying to send " +
                                                $"{nameof(error.FailureMessage)} to the user."); 
                        });
                }

                if (error.Exception != null)
                {
                    logger.LogDebug(error.Exception, 
                        "Next, some details to help debug the current exception. " +
                        "BotType: '{botType}'; Telegram user Id: '{userId}'; " +
                        "DateTime of received Message: '{telegramDate}'; with text: '{text}'",
                        botType, update.Message.From!.Id,
                        update.Message.Date, update.Message.Text);
                }
                
                return error;
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
    
    private async Task<Attempt<Unit>> SendOutputsAsync(
        IReadOnlyList<OutputDto> outputs, IBotClientWrapper botClient, ChatId chatId)
    {
        if (_uiTranslator == null)
            throw new InvalidOperationException("UiTranslator or translated OutputMessage must not be NULL.");

        if (_replyMarkupConverter == null)
            throw new InvalidOperationException("ReplyMarkupConverter must not be null.");

        return await Attempt<Unit>.RunAsync(async () =>
        {
            var parallelTasks = outputs.Select(async output =>
                await botClient.SendTextMessageOrThrowAsync(
                    chatId,
                    _uiTranslator.Translate(Ui("Please choose:")),
                    _uiTranslator.Translate(output.Text.GetValueOrDefault()),
                    _replyMarkupConverter.GetReplyMarkup(output)));

            // 1) Waits for all tasks, which started executing in parallel in the .Select() iteration, to complete
            // 2) Once all completed, rethrows any Exception that might have occurred in any one task's execution. 
            await Task.WhenAll(parallelTasks);
            
            return Unit.Value;
        });
    }
}