using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.ExtAPIs.AzureServices;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Bot.Telegram.BotClient;
using CheckMade.Bot.Telegram.Conversion;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.Logging;

namespace CheckMade.Bot.Telegram.UpdateHandling;

public interface IUpdateHandler
{
    Task HandleUpdateAsync(
        UpdateWrapper update, InteractionMode currentInteractionMode);
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
    public async Task HandleUpdateAsync(
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

        var botClientByMode = new Dictionary<InteractionMode, IBotClientWrapper>
        {
            { InteractionMode.Operations, botClientFactory.CreateBotClient(InteractionMode.Operations) },
            { InteractionMode.Communications, botClientFactory.CreateBotClient(InteractionMode.Communications) },
            { InteractionMode.Notifications, botClientFactory.CreateBotClient(InteractionMode.Notifications) }
        };

        try
        {
            var toModelConverter = toModelConverterFactory.Create(
                new TelegramFilePathResolver(botClientByMode[currentInteractionMode]));
            
            var input =
                await toModelConverter.ConvertToModelAsync(update, currentInteractionMode);

            var result = await input.MatchAsync(
                
                async i => await inputProcessor.ProcessInputAsync(i), 
                
                static failure => 
                    (Option<Input>.None(),
                    [
                        new Output
                        {
                            Text = failure is ExceptionWrapper 
                                ? UiNoTranslate(failure.GetEnglishMessage()) 
                                : ((BusinessError)failure).Error
                        }]));
            
            var activeRoleBindings = 
                await agentRoleBindingsRepo.GetAllActiveAsync();
            
            var uiTranslator = 
                translatorFactory.Create(GetUiLanguage(activeRoleBindings, currentAgent));
            
            var replyMarkupConverter = 
                replyMarkupConverterFactory.Create(uiTranslator, glossary);
            
            var sentOutputs = await OutputSender.SendOutputsAsync(
                result.ResultingOutputs, botClientByMode, currentAgent, activeRoleBindings,
                uiTranslator, replyMarkupConverter, blobLoader, msgIdCache, glossary, logger);

            await SaveToDbAsync(result.EnrichedOriginalInput, sentOutputs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception with message '{exMessage}' was thrown. " +
                                "Next, some details to help debug the current exception. " +
                                "InteractionMode: '{interactionMode}'; Telegram User Id: '{userId}'; " +
                                "DateTime of received Update: '{telegramDate}'; with text: '{text}'",
                ex.Message, 
                currentInteractionMode, 
                update.Message.From!.Id,
                update.Message.Date,
                update.Message.Text);
        }
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

    private async Task SaveToDbAsync(
        Option<Input> enrichedInput, IReadOnlyCollection<Result<Output>> sentOutputs)
    {
        if (enrichedInput.IsNone)
            return;
        
        await inputsRepo.AddAsync(
            enrichedInput.GetValueOrThrow(), 
            GetDestinationInfoForWorkflowBridges());

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