using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using Microsoft.Extensions.Logging;

namespace CheckMade.Bot.Workflows;

public sealed class InputProcessor(
    IWorkflowIdentifier workflowIdentifier,
    IGeneralWorkflowUtils workflowUtils,
    IDomainGlossary glossary,
    ILastOutputMessageIdCache msgIdCache,
    ILogger<InputProcessor> logger)
    : IInputProcessor
{
    public async Task<(Option<Input> EnrichedOriginalInput, IReadOnlyCollection<Output> ResultingOutputs)> 
        ProcessInputAsync(Result<Input> input)
    {
        return await input.Match(
            async currentInput =>
            {
                if (currentInput.InputType == InputType.Location)
                {
                    return (currentInput, []);
                }
                
                List<Output> outputBuilder = [];

                if (IsStartCommand(currentInput))
                {
                    outputBuilder.Add(new Output
                    { 
                        Text = Ui("🫡 Welcome to the CheckMade Bot. " +
                                  "I shall follow your command!")
                    });
                }
                
                if (await IsInputInterruptingPreviousWorkflowAsync(currentInput))
                {
                    outputBuilder.Add(
                        new Output
                        {
                            Text = Ui("FYI: you interrupted the previous workflow before its completion or " +
                                      "successful submission."),
                            UpdateExistingOutputMessageId = msgIdCache.GetLastMessageId(currentInput.Agent)
                        });
                }

                var inputHistory = 
                    await workflowUtils.GetAllCurrentInteractiveAsync(currentInput.Agent, currentInput);
                
                var activeWorkflow = 
                    await workflowIdentifier.IdentifyAsync(inputHistory);
                
                var responseResult = 
                    await GetResponseFromActiveWorkflowAsync(activeWorkflow, currentInput);

                var enrichedCurrentInput = currentInput with
                {
                    ResultantState = GetResultantWorkflowState(responseResult, activeWorkflow)
                                     ?? Option<ResultantWorkflowState>.None(),
                    EntityGuid = GetEntityGuid(responseResult)
                };
                
                return (
                    Option<Input>.Some(enrichedCurrentInput), 
                    ResolveResponseResultIntoOutputs(
                        responseResult,
                        outputBuilder,
                        activeWorkflow,
                        currentInput));
            },
            static failure =>
            {
                var failureOutput = failure switch
                {
                    ExceptionWrapper exw => new Output { Text = UiNoTranslate(exw.Exception.Message) },
                    _ => new Output { Text = ((BusinessError)failure).Error }
                };

                return Task.FromResult<(Option<Input> EnrichedOriginalInput,
                    IReadOnlyCollection<Output> ResultingOutputs)>((
                    Option<Input>.None(), [failureOutput]
                ));
            });
    }

    private static bool IsStartCommand(Input currentInput) =>
        currentInput.InputType.Equals(InputType.CommandMessage)
        && currentInput.Details.BotCommandEnumCode.Equals(Start.CommandCode);
    
    private ResultantWorkflowState? GetResultantWorkflowState(
        Result<WorkflowResponse> response,
        Option<WorkflowBase> activeWorkflow)
    {
        ResultantWorkflowState? workflowInfo = null;
                
        var newState = response.Match(
            static r => r.NewStateId,
            static _ => Option<string>.None());
                
        if (activeWorkflow.IsSome && newState.IsSome)
        {
            workflowInfo = new ResultantWorkflowState(
                glossary.GetId(activeWorkflow.GetValueOrThrow().GetType()),
                newState.GetValueOrThrow());
        }

        return workflowInfo;
    }

    private static Option<Guid> GetEntityGuid(Result<WorkflowResponse> response) =>
        response.Match(
            static r => r.EntityGuid,
            static _ => Option<Guid>.None());
    
    private async Task<bool> IsInputInterruptingPreviousWorkflowAsync(Input currentInput)
    {
        if (currentInput.OriginatorRole.IsNone)
            return false;
        
        if (currentInput.InputType is not InputType.CommandMessage)
            return false;
        
        var previousWorkflowInputHistory = 
            (await workflowUtils.GetAllCurrentInteractiveAsync(currentInput.Agent, currentInput))
            .SkipLast(1) // Excluding the current BotCommand input
            .GetLatestRecordsUpTo(static input => 
                input.InputType.Equals(InputType.CommandMessage))
            .ToList();

        return previousWorkflowInputHistory.Count > 0 && 
               !IsWorkflowTerminated(previousWorkflowInputHistory);
    }
    
    private bool IsWorkflowTerminated(IReadOnlyCollection<Input> inputHistory)
    {
        return
            inputHistory.Any(i =>
                i.ResultantState.IsSome &&
                glossary.GetDtType(
                        i.ResultantState.GetValueOrThrow().InStateId)
                    .IsAssignableTo(typeof(IWorkflowStateTerminator)));
    }

    private static async Task<Result<WorkflowResponse>>
        GetResponseFromActiveWorkflowAsync(
            Option<WorkflowBase> activeWorkflow,
            Input currentInput)
    {
        return await activeWorkflow.Match(
            wf => 
                wf.GetResponseAsync(currentInput),
            static () => 
                Task.FromResult(Result<WorkflowResponse>
                    .Succeed(new WorkflowResponse(
                        [
                            new Output 
                            { 
                                Text = IInputProcessor.SeeValidBotCommandsInstruction
                            }
                        ], 
                        Option<string>.None(),
                        Option<Guid>.None()))));
    }

    private IReadOnlyCollection<Output> ResolveResponseResultIntoOutputs(
        Result<WorkflowResponse> responseResult,
        List<Output> outputBuilder,
        Option<WorkflowBase> activeWorkflow,
        Input currentInput)
    {
        return responseResult.Match(
            wr => 
            {
                outputBuilder.AddRange(wr.Output);
                return outputBuilder;
            },
            failure =>
            {
                switch (failure)
                {
                    case ExceptionWrapper exw:
                        logger.LogError($"""
                                         The workflow '{activeWorkflow.GetValueOrDefault().GetType()}' has returned
                                         this exception: '{exw.Exception.Message}'. Next, the corresponding input parameters.
                                         UserId: {currentInput.Agent.UserId}; ChatId: {currentInput.Agent.ChatId}; 
                                         InputType: {currentInput.InputType}; InteractionMode: {currentInput.Agent.Mode};
                                         Date: {currentInput.TimeStamp}; 
                                         For more details of input, check database!
                                         """);
                        throw exw.Exception;
                    
                    default:
                        return [new Output { Text = ((BusinessError)failure).Error }];
                }
            }
        );
    }
}
