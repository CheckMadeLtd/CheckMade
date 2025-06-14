using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Function.Startup;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.DomainModel.ChatBot;
using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.ChatBot.Output;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Core;
using CheckMade.Common.DomainModel.Interfaces.ChatBotFunction;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.DomainModel.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

public interface IUpdateHandler
{
    Task<Result<Unit>> HandleUpdateAsync(UpdateWrapper update, InteractionMode currentInteractionMode);
}

public sealed class UpdateHandler(
    IBotClientFactory botClientFactory,
    IInputProcessor inputProcessor,
    ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo,
    IToModelConverterFactory toModelConverterFactory,
    DefaultUiLanguageCodeProvider defaultUiLanguage,
    IUiTranslatorFactory translatorFactory,
    IOutputToReplyMarkupConverterFactory replyMarkupConverterFactory,
    IBlobLoader blobLoader,
    ITlgInputsRepository inputsRepo,
    IDomainGlossary glossary,
    ILastOutputMessageIdCache msgIdCache,
    ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task<Result<Unit>> HandleUpdateAsync(
        UpdateWrapper update,
        InteractionMode currentInteractionMode)
    {
        var currentTlgAgent = new TlgAgent(
            // Assuming Message.From will never be null for us, because this only happens for e.g. 
            // Annonymous admins in groups and posts in Channels, neither of which are planned for CheckMade
            update.Message.From!.Id,
            update.Message.Chat.Id,
            currentInteractionMode);
        
        logger.LogTrace("Invoked telegram update function for InteractionMode: {interactionMode} " + 
                        "with Message from UserId/ChatId: {userId}/{chatId}", 
            currentInteractionMode, 
            currentTlgAgent.UserId,
            currentTlgAgent.ChatId);

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
            { InteractionMode.Operations, botClientFactory.CreateBotClient(InteractionMode.Operations) },
            { InteractionMode.Communications, botClientFactory.CreateBotClient(InteractionMode.Communications) },
            { InteractionMode.Notifications, botClientFactory.CreateBotClient(InteractionMode.Notifications) }
        };
        
        var handleUpdateAttempt = await
            (from toModelConverter
                    in Result<IToModelConverter>.Run(() => 
                        toModelConverterFactory.Create(
                            new TelegramFilePathResolver(botClientByMode[currentInteractionMode])))
                from tlgInput
                    // this nested Result is necessary so that a failed conversion gets wrapped in a successful Result
                    // to ensure it gets passed on to the next step where any Exception or BusinessError is turned 
                    // into user-facing output
                    in Result<Result<TlgInput>>.RunAsync(() =>
                        toModelConverter.ConvertToModelAsync(update, currentInteractionMode))
                from result
                    in Result<(Option<TlgInput> EnrichedOriginalInput, 
                        IReadOnlyCollection<OutputDto> ResultingOutputs)>.RunAsync(() => 
                        inputProcessor.ProcessInputAsync(tlgInput))
                from activeRoleBindings
                    in Result<IReadOnlyCollection<TlgAgentRoleBind>>.RunAsync(async () => 
                        (await tlgAgentRoleBindingsRepo.GetAllActiveAsync())
                        .ToArray())
                from uiTranslator
                    in Result<IUiTranslator>.Run(() => 
                        translatorFactory.Create(GetUiLanguage(activeRoleBindings, currentTlgAgent)))
                from replyMarkupConverter
                    in Result<IOutputToReplyMarkupConverter>.Run(() => 
                        replyMarkupConverterFactory.Create(uiTranslator))
                from sentOutputs
                    in Result<IReadOnlyCollection<Result<OutputDto>>>.RunAsync(() => 
                        OutputSender.SendOutputsAsync(
                            result.ResultingOutputs, botClientByMode, currentTlgAgent, activeRoleBindings, 
                            uiTranslator, replyMarkupConverter, blobLoader, msgIdCache, logger))
                from unit 
                    in Result<Unit>.RunAsync(() =>
                        SaveToDbAsync(result.EnrichedOriginalInput, sentOutputs))
                select unit);
        
        return handleUpdateAttempt.Match(
            static _ => Result<Unit>.Succeed(Unit.Value),
            failure =>
            {
                switch (failure)
                {
                    case ExceptionWrapper exw:
                        logger.LogError(exw.Exception, "Exception with message '{exMessage}' was thrown. " +
                                                       "Next, some details to help debug the current exception. " +
                                                       "InteractionMode: '{interactionMode}'; Telegram User Id: '{userId}'; " +
                                                       "DateTime of received Update: '{telegramDate}'; with text: '{text}'",
                            exw.GetEnglishMessage(), 
                            currentInteractionMode, 
                            update.Message.From!.Id,
                            update.Message.Date,
                            update.Message.Text);

                        return failure;
                    
                    default:
                        var error = (BusinessError)failure;
                        logger.LogWarning("The following {businessError} was returned: '{message}'",
                            nameof(BusinessError),
                            error.GetEnglishMessage());
                        
                        return failure;
                }
            });
    }

    private LanguageCode GetUiLanguage(
        IReadOnlyCollection<TlgAgentRoleBind> activeRoleBindings,
        TlgAgent currentTlgAgent)
    {
        var tlgAgentRole = activeRoleBindings
            .FirstOrDefault(tarb =>
                tarb.TlgAgent.Equals(currentTlgAgent));
        
        return tlgAgentRole != null 
            ? tlgAgentRole.Role.ByUser.Language 
            : defaultUiLanguage.Code;
    }

    private async Task<Unit> SaveToDbAsync(
        Option<TlgInput> enrichedInput, IReadOnlyCollection<Result<OutputDto>> sentOutputs)
    {
        if (enrichedInput.IsNone)
            return Unit.Value;
        
        await inputsRepo.AddAsync(
            enrichedInput.GetValueOrThrow(), 
            GetDestinationInfoForWorkflowBridges());
        
        return Unit.Value;

        Option<IReadOnlyCollection<ActualSendOutParams>> GetDestinationInfoForWorkflowBridges()
        {
            var resultantWorkflowState = 
                enrichedInput.GetValueOrThrow().ResultantState; 
        
            var doesCurrentInputTerminateWorkflow =
                resultantWorkflowState.IsSome &&
                glossary.GetDtType(resultantWorkflowState.GetValueOrThrow().InStateId)
                    .IsAssignableTo(typeof(IWorkflowStateTerminator));

            if (!doesCurrentInputTerminateWorkflow)
                return Option<IReadOnlyCollection<ActualSendOutParams>>.None();
            
            Func<Result<OutputDto>, bool> isOtherRecipientThanOriginatingRole = outputAttempt => 
                outputAttempt.Match(
                    o => o.LogicalPort.IsSome &&
                         !enrichedInput.GetValueOrThrow().OriginatorRole.GetValueOrThrow()
                             .Equals(o.LogicalPort.GetValueOrThrow().Role),
                    static _ => false);

            return Option<IReadOnlyCollection<ActualSendOutParams>>.Some(
                sentOutputs
                    .Where(isOtherRecipientThanOriginatingRole)
                    .Select(static o => o.GetValueOrThrow().ActualSendOutParams!.Value)
                    .ToArray());
        }
    }
}