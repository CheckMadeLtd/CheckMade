using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Logic.RequestProcessors;
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
        IRequestProcessorSelector selector,
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

        var botClientByBotType = new Dictionary<BotType, IBotClientWrapper>
        {
            { BotType.Operations,  botClientFactory.CreateBotClientOrThrow(BotType.Operations) },
            { BotType.Communications, botClientFactory.CreateBotClientOrThrow(BotType.Communications) },
            { BotType.Notifications, botClientFactory.CreateBotClientOrThrow(BotType.Notifications) }
        };
        
        var filePathResolver = new TelegramFilePathResolver(botClientByBotType[updateReceivingBotType]);
        var toModelConverter = toModelConverterFactory.Create(filePathResolver);
        var chatIdByOutputDestination = await GetChatIdByOutputDestinationAsync();
        var uiTranslator = translatorFactory.Create(GetUiLanguage(update.Message));
        var replyMarkupConverter = replyMarkupConverterFactory.Create(uiTranslator);
        
        return (await 
                (from telegramUpdate 
                        in await toModelConverter.ConvertToModelAsync(update, updateReceivingBotType)
                from outputs 
                    in selector.GetRequestProcessor(updateReceivingBotType).ProcessRequestAsync(telegramUpdate)
                select 
                    SendOutputsAsync(
                        outputs, 
                        botClientByBotType,
                        updateReceivingBotType,
                        updateReceivingChatId,
                        chatIdByOutputDestination,
                        uiTranslator,
                        replyMarkupConverter,
                        blobLoader)))
            .Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            error =>
            {
                if (error.FailureMessage != null)
                {
                    logger.LogWarning($"Message to User from {nameof(error.FailureMessage)}: " +
                                    $"{error.FailureMessage.GetFormattedEnglish()}");
                    
                    _ = SendOutputsAsync(
                            new List<OutputDto>{ OutputDto.Create(error.FailureMessage) }, 
                            botClientByBotType,
                            updateReceivingBotType,
                            updateReceivingChatId,
                            chatIdByOutputDestination,
                            uiTranslator,
                            replyMarkupConverter,
                            blobLoader)
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
                        "DateTime of received Update: '{telegramDate}'; with text: '{text}'",
                        updateReceivingBotType, update.Message.From!.Id,
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
    
    private async Task<IDictionary<TelegramOutputDestination, TelegramChatId>> GetChatIdByOutputDestinationAsync() =>
        (await Attempt<IDictionary<TelegramOutputDestination, TelegramChatId>>.RunAsync(async () =>
            (await chatIdByOutputDestinationRepository.GetAllOrThrowAsync())
            .ToDictionary(
                keySelector: map => map.OutputDestination,
                elementSelector: map => map.ChatId)))
        .Match(
            value => value,
            error => throw new DataAccessException($"An exception was thrown while trying to access " + 
                                                   $"{nameof(chatIdByOutputDestinationRepository)}", error.Exception));
    
    private static async Task<Attempt<Unit>> SendOutputsAsync(
        IReadOnlyList<OutputDto> outputs,
        IDictionary<BotType, IBotClientWrapper> botClientByBotType,
        BotType updateReceivingBotType,
        ChatId updateReceivingChatId,
        IDictionary<TelegramOutputDestination, TelegramChatId> chatIdByOutputDestination,
        IUiTranslator uiTranslator,
        IOutputToReplyMarkupConverter converter,
        IBlobLoader blobLoader)
    {
        return await Attempt<Unit>.RunAsync(async () =>
        {
            var parallelTasks = outputs.Select(async output =>
            {
                var destinationBotClient = output.ExplicitDestination.IsSome
                    ? botClientByBotType[output.ExplicitDestination.Value!.DestinationBotType]
                    : botClientByBotType[updateReceivingBotType]; // e.g. for a virgin, pre-login update

                var destinationChatId = output.ExplicitDestination.IsSome
                    ? chatIdByOutputDestination[output.ExplicitDestination.Value!].Id
                    : updateReceivingChatId; // e.g. for a virgin, pre-login update

                switch (output)
                {
                    case { Attachments.IsSome: false }:
                        await SendTextMessageAsync();
                        break;

                    case { Attachments.IsSome: true }:
                        await Task.WhenAll(
                            output.Attachments.Value!
                                .Select(details => details.AttachmentType switch
                                {
                                    AttachmentType.Photo => SendPhotoAsync(details),
                                    AttachmentType.Audio or AttachmentType.Document =>
                                        throw new InvalidOperationException("Not yet supported attachment type"),
                                    _ => 
                                        throw new InvalidOperationException("Not yet supported attachment type")
                                }));
                        break;
                    
                    case { Location.IsSome: true }:
                        break;
                }
                
                return;

                async Task SendTextMessageAsync()
                {
                    await destinationBotClient
                        .SendTextMessageOrThrowAsync(
                            destinationChatId,
                            uiTranslator.Translate(Ui("Please choose:")),
                            uiTranslator.Translate(output.Text.GetValueOrDefault(Ui())),
                            converter.GetReplyMarkup(output));
                }
                
                async Task SendPhotoAsync(OutputAttachmentDetails details)
                {
                    var (blobData, fileName) = 
                        await blobLoader.DownloadBlobAsync(details.AttachmentUri.AbsoluteUri);
                    var fileStream = new InputFileStream(blobData, fileName);
                    
                    await destinationBotClient.SendPhotoOrThrowAsync(
                        destinationChatId,
                        fileStream,
                        uiTranslator.Translate(output.Text.GetValueOrDefault(Ui())),
                        converter.GetReplyMarkup(output)
                    );
                }  
            });
            
            /* FYI about Task.WhenAll() behaviour here
             * 1) Waits for all tasks, which started executing in parallel in the .Select() iteration, to complete
             * 2) Once all completed, rethrows any Exception that might have occurred in any one task's execution. */ 
            await Task.WhenAll(parallelTasks);
            
            return Unit.Value;
        });
    }
}