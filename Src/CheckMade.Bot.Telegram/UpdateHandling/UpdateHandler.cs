using CheckMade.Abstract.Domain.Data.Bot;
using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Data.Bot.Output;
using CheckMade.Abstract.Domain.Data.Bot.UserInteraction;
using CheckMade.Abstract.Domain.Interfaces.Bot.Function;
using CheckMade.Abstract.Domain.Interfaces.Bot.Logic;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Bot;
using CheckMade.Bot.Telegram.BotClient;
using CheckMade.Bot.Telegram.Conversion;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Bot.Telegram.UpdateHandling;

public interface IUpdateHandler
{
    Task<Result<Unit>> HandleUpdateAsync(UpdateWrapper update, InteractionMode currentInteractionMode);
}

public sealed class UpdateHandler(
    IBotClientFactory botClientFactory,
    IInputProcessor inputProcessor,
    IAgentRoleBindingsRepository agentRoleBindingsRepo,
    IToModelConverterFactory toModelConverterFactory,
    DefaultUiLanguageCodeProvider defaultUiLanguage,
    IUiTranslatorFactory translatorFactory,
    IOutputToReplyMarkupConverterFactory replyMarkupConverterFactory,
    IBlobLoader blobLoader,
    IInputsRepository inputsRepo,
    IDomainGlossary glossary,
    ILastOutputMessageIdCache msgIdCache,
    ILogger<UpdateHandler> logger)
    : IUpdateHandler
{
    public async Task<Result<Unit>> HandleUpdateAsync(
        UpdateWrapper update,
        InteractionMode currentInteractionMode)
    {
        var currentAgent = new Agent(
            // Assuming Message.From will never be null for us, because this only happens for e.g. 
            // Annonymous admins in groups and posts in Channels, neither of which are planned for CheckMade
            update.Message.From!.Id,
            update.Message.Chat.Id,
            currentInteractionMode);
        
        logger.LogTrace("Invoked telegram update function for InteractionMode: {interactionMode} " + 
                        "with Message from UserId/ChatId: {userId}/{chatId}", 
            currentInteractionMode, 
            currentAgent.UserId,
            currentAgent.ChatId);

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
                from input
                    // this nested Result is necessary so that a failed conversion gets wrapped in a successful Result
                    // to ensure it gets passed on to the next step where any Exception or BusinessError is turned 
                    // into user-facing output
                    in Result<Result<Input>>.RunAsync(() =>
                        toModelConverter.ConvertToModelAsync(update, currentInteractionMode))
                from result
                    in Result<(Option<Input> EnrichedOriginalInput, 
                        IReadOnlyCollection<Output> ResultingOutputs)>.RunAsync(() => 
                        inputProcessor.ProcessInputAsync(input))
                from activeRoleBindings
                    in Result<IReadOnlyCollection<AgentRoleBind>>.RunAsync(async () => 
                        (await agentRoleBindingsRepo.GetAllActiveAsync())
                        .ToArray())
                from uiTranslator
                    in Result<IUiTranslator>.Run(() => 
                        translatorFactory.Create(GetUiLanguage(activeRoleBindings, currentAgent)))
                from replyMarkupConverter
                    in Result<IOutputToReplyMarkupConverter>.Run(() => 
                        replyMarkupConverterFactory.Create(uiTranslator, glossary))
                from sentOutputs
                    in Result<IReadOnlyCollection<Result<Output>>>.RunAsync(() => 
                        OutputSender.SendOutputsAsync(
                            result.ResultingOutputs, botClientByMode, currentAgent, activeRoleBindings, 
                            uiTranslator, replyMarkupConverter, blobLoader, msgIdCache, glossary, logger))
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
        IReadOnlyCollection<AgentRoleBind> activeRoleBindings,
        Agent currentAgent)
    {
        var agentRoleBind = activeRoleBindings
            .FirstOrDefault(arb =>
                arb.Agent.Equals(currentAgent));
        
        return agentRoleBind != null 
            ? agentRoleBind.Role.ByUser.Language 
            : defaultUiLanguage.Code;
    }

    private async Task<Unit> SaveToDbAsync(
        Option<Input> enrichedInput, IReadOnlyCollection<Result<Output>> sentOutputs)
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
            
            Func<Result<Output>, bool> isOtherRecipientThanOriginatingRole = outputAttempt => 
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