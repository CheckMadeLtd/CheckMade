using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Logic.UpdateProcessors;
using CheckMade.Telegram.Model.DTOs;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services.UpdateHandling;

public interface IUpdateHandler
{
    Task<Attempt<Unit>> HandleUpdateAsync(UpdateWrapper update, BotType botType);
}

public class UpdateHandler(
        IBotClientFactory botClientFactory,
        IUpdateProcessorSelector selector,
        IChatIdByOutputDestinationRepository chatIdByOutputDestinationRepository,
        IToModelConverterFactory toModelConverterFactory,
        DefaultUiLanguageCodeProvider defaultUiLanguage,
        IUiTranslatorFactory translatorFactory,
        IOutputToReplyMarkupConverterFactory replyMarkupConverterFactory,
        IBlobLoader blobLoader,
        ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task<Attempt<Unit>> HandleUpdateAsync(UpdateWrapper update, BotType updateReceivingBotType)
    {
        ChatId updateReceivingChatId = update.Message.Chat.Id;
        
        logger.LogTrace("Invoked telegram update function for BotType: {botType} " +
                              "with Message from UserId/ChatId: {userId}/{chatId}", 
            updateReceivingBotType, update.Message.From?.Id ?? 0, updateReceivingChatId);

        var handledMessageTypes = new[]
        {
            MessageType.Document,
            MessageType.Location,
            MessageType.Photo,
            MessageType.Text,
            MessageType.Voice
        };

        if (!handledMessageTypes.Contains(update.Message.Type))
        {
            logger.LogWarning("Received message of type '{messageType}': {warning}", 
                update.Message.Type, BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish());

            return Unit.Value;
        }

        var botClientByBotType = new Dictionary<BotType, IBotClientWrapper>
        {
            { BotType.Operations,  botClientFactory.CreateBotClient(BotType.Operations) },
            { BotType.Communications, botClientFactory.CreateBotClient(BotType.Communications) },
            { BotType.Notifications, botClientFactory.CreateBotClient(BotType.Notifications) }
        };
        
        var filePathResolver = new TelegramFilePathResolver(botClientByBotType[updateReceivingBotType]);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver);
        var uiTranslator = translatorFactory.Create(GetUiLanguage(update.Message));
        var replyMarkupConverter = replyMarkupConverterFactory.Create(uiTranslator);

        var sendOutputsAttempt = await
            (from telegramUpdate
                    in Attempt<Result<TelegramUpdate>>.RunAsync(() => 
                        toModelConverter.ConvertToModelAsync(update, updateReceivingBotType))
                from outputs
                    in Attempt<IReadOnlyList<OutputDto>>.RunAsync(() => 
                        selector.GetUpdateProcessor(updateReceivingBotType).ProcessUpdateAsync(telegramUpdate))
                from chatIdByOutputDestination
                    in Attempt<IDictionary<TelegramOutputDestination, TelegramChatId>>.RunAsync(
                        GetChatIdByOutputDestinationAsync) 
                from unit
                  in Attempt<Unit>.RunAsync(() => 
                      OutputSender.SendOutputsAsync(
                          outputs, botClientByBotType, updateReceivingBotType, updateReceivingChatId,
                          chatIdByOutputDestination, uiTranslator, replyMarkupConverter, blobLoader)) 
                select unit);
        
        return sendOutputsAttempt.Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            ex =>
            {
                logger.LogError(ex, "Exception with message '{exMessage}' was thrown. " +
                                    "Next, some details to help debug the current exception. " +
                                    "BotType: '{botType}'; Telegram user Id: '{userId}'; " +
                                    "DateTime of received Update: '{telegramDate}'; with text: '{text}'",
                    ex.Message, updateReceivingBotType, update.Message.From!.Id,
                    update.Message.Date, update.Message.Text);
                
                return ex;
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
    
    private async Task<IDictionary<TelegramOutputDestination, TelegramChatId>> GetChatIdByOutputDestinationAsync() =>
        (await chatIdByOutputDestinationRepository.GetAllAsync())
            .ToDictionary(
                keySelector: map => map.OutputDestination,
                elementSelector: map => map.ChatId);
}