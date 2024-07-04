using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Function.Startup;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

public interface IUpdateHandler
{
    Task<Attempt<Unit>> HandleUpdateAsync(UpdateWrapper update, InteractionMode currentlyReceivingInteractionMode);
}

public class UpdateHandler(
        IBotClientFactory botClientFactory,
        IInputProcessor inputProcessor,
        ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo,
        IToModelConverterFactory toModelConverterFactory,
        DefaultUiLanguageCodeProvider defaultUiLanguage,
        IUiTranslatorFactory translatorFactory,
        IOutputToReplyMarkupConverterFactory replyMarkupConverterFactory,
        IBlobLoader blobLoader,
        ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task<Attempt<Unit>> HandleUpdateAsync(
        UpdateWrapper update,
        InteractionMode currentlyReceivingInteractionMode)
    {
        var currentlyReceivingUserId = update.Message.From?.Id;
        ChatId currentlyReceivingChatId = update.Message.Chat.Id;
        
        logger.LogTrace("Invoked telegram update function for InteractionMode: {interactionMode} " + 
                        "with Message from UserId/ChatId: {userId}/{chatId}", 
            currentlyReceivingInteractionMode, 
            update.Message.From?.Id ?? 0,
            currentlyReceivingChatId);

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

        var botClientByMode = new Dictionary<InteractionMode, IBotClientWrapper>
        {
            { InteractionMode.Operations,  botClientFactory.CreateBotClient(InteractionMode.Operations) },
            { InteractionMode.Communications, botClientFactory.CreateBotClient(InteractionMode.Communications) },
            { InteractionMode.Notifications, botClientFactory.CreateBotClient(InteractionMode.Notifications) }
        };
        
        var sendOutputsAttempt = await
            (from toModelConverter
                    in Attempt<IToModelConverter>.Run(() => 
                        toModelConverterFactory.Create(
                            new TelegramFilePathResolver(botClientByMode[currentlyReceivingInteractionMode])))
                from tlgInput
                    in Attempt<Result<TlgInput>>.RunAsync(() => 
                        toModelConverter.ConvertToModelAsync(update, currentlyReceivingInteractionMode))
                from outputs
                    in Attempt<IReadOnlyCollection<OutputDto>>.RunAsync(() => 
                        inputProcessor.ProcessInputAsync(tlgInput))
                from activeRoleBindings
                    in Attempt<IReadOnlyCollection<TlgAgentRoleBind>>.RunAsync(async () => 
                        (await tlgAgentRoleBindingsRepo.GetAllActiveAsync())
                        .ToImmutableReadOnlyCollection())
                from uiTranslator
                    in Attempt<IUiTranslator>.Run(() => 
                        translatorFactory.Create(GetUiLanguage(
                            activeRoleBindings,
                            currentlyReceivingUserId,
                            currentlyReceivingChatId,
                            currentlyReceivingInteractionMode)))
                from replyMarkupConverter
                    in Attempt<IOutputToReplyMarkupConverter>.Run(() => 
                        replyMarkupConverterFactory.Create(uiTranslator))
                from unit
                  in Attempt<Unit>.RunAsync(() => 
                      OutputSender.SendOutputsAsync(
                          outputs, botClientByMode, currentlyReceivingInteractionMode, currentlyReceivingChatId,
                          activeRoleBindings, uiTranslator, replyMarkupConverter, blobLoader)) 
                select unit);
        
        return sendOutputsAttempt.Match(
            
            _ => Attempt<Unit>.Succeed(Unit.Value),

            ex =>
            {
                logger.LogError(ex, "Exception with message '{exMessage}' was thrown. " +
                                    "Next, some details to help debug the current exception. " +
                                    "InteractionMode: '{interactionMode}'; Telegram User Id: '{userId}'; " +
                                    "DateTime of received Update: '{telegramDate}'; with text: '{text}'",
                    ex.Message, 
                    currentlyReceivingInteractionMode, 
                    update.Message.From!.Id,
                    update.Message.Date,
                    update.Message.Text);
                
                return ex;
            });
    }

    private LanguageCode GetUiLanguage(
        IReadOnlyCollection<TlgAgentRoleBind> activeRoleBindings,
        long? currentUserId,
        ChatId currentChatId,
        InteractionMode currentMode)
    {
        var tlgAgentRole = activeRoleBindings
            .FirstOrDefault(tarb =>
                tarb.TlgAgent.UserId.Id == currentUserId &&
                tarb.TlgAgent.ChatId.Id == currentChatId &&
                tarb.TlgAgent.Mode == currentMode);

        return tlgAgentRole != null 
            ? tlgAgentRole.Role.ByUser.Language 
            : defaultUiLanguage.Code;
    }
}