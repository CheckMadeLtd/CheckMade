using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> input);
}

internal class InputProcessor(
        IWorkflowIdentifier workflowIdentifier,
        ITlgInputsRepository inputsRepo,
        ILogicUtils logicUtils,
        IDomainGlossary glossary,
        ILogger<InputProcessor> logger)
    : IInputProcessor
{
    public async Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> input)
    {
        return await input.Match(
            async currentInput =>
            {
                if (currentInput.InputType == TlgInputType.Location)
                {
                    await SaveCurrentInputToDbAsync(currentInput);
                    
                    return [];
                }
                
                List<OutputDto> outputBuilder = [];

                if (IsStartCommand(currentInput))
                {
                    outputBuilder.Add(new OutputDto{ Text = Ui("🫡 Welcome to the CheckMade ChatBot. " +
                                                               "I shall follow your command!") });
                }
                
                if (await IsInputInterruptingPreviousWorkflowAsync(currentInput))
                {
                    outputBuilder.Add(new OutputDto
                        {
                            Text = Ui("FYI: you interrupted the previous workflow before its completion or " +
                                      "successful submission.")
                        });
                }

                var activeWorkflowInputHistory = 
                    await logicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
                
                if (IsCurrentInputFromOutOfScopeWorkflow(currentInput, activeWorkflowInputHistory))
                {
                    await SaveCurrentInputToDbAsync(currentInput);
                    
                    return 
                        [new OutputDto 
                        {
                            Text = Ui("The previous workflow was completed, " +
                                      "so your last message/action will be ignored.") 
                        }];
                }

                var activeWorkflow = 
                    workflowIdentifier.Identify(activeWorkflowInputHistory);
                
                var responseResult = 
                    await GetResponseFromActiveWorkflowAsync(activeWorkflow, currentInput);
                
                await SaveCurrentInputToDbAsync(
                    currentInput,
                    GetResultantWorkflowInfo(responseResult, activeWorkflow));

                return ResolveResponseResultIntoOutputs(
                    responseResult,
                    outputBuilder,
                    activeWorkflow,
                    currentInput);
            },
            // This error was already logged at its source, in ToModelConverter
            error => 
                Task.FromResult<IReadOnlyCollection<OutputDto>>(
                    [new OutputDto { Text = error }]));
    }

    private static bool IsStartCommand(TlgInput currentInput) =>
        currentInput.InputType.Equals(TlgInputType.CommandMessage)
        && currentInput.Details.BotCommandEnumCode.Equals(TlgStart.CommandCode);
    
    private ResultantWorkflowInfo? GetResultantWorkflowInfo(
        Result<WorkflowResponse> response,
        Option<IWorkflow> activeWorkflow)
    {
        ResultantWorkflowInfo? workflowInfo = null;
                
        var newState = response.Match(
            r => r.NewStateId,
            _ => Option<string>.None());
                
        if (activeWorkflow.IsSome && newState.IsSome)
        {
            workflowInfo = new ResultantWorkflowInfo(
                glossary.GetId(activeWorkflow.GetValueOrThrow().GetType()),
                newState.GetValueOrThrow());
        }

        return workflowInfo;
    }
    
    private async Task SaveCurrentInputToDbAsync(
        TlgInput currentInput,
        ResultantWorkflowInfo? workflowInfo = null)
    {
        await inputsRepo.AddAsync(currentInput with
        {
            ResultantWorkflow = workflowInfo ?? Option<ResultantWorkflowInfo>.None()
        });
    }
    
    private async Task<bool> IsInputInterruptingPreviousWorkflowAsync(TlgInput currentInput)
    {
        if (currentInput.OriginatorRole.IsNone)
            return false;
        
        if (currentInput.InputType is not TlgInputType.CommandMessage)
            return false;
        
        var previousWorkflowInputHistory = 
            (await logicUtils.GetAllCurrentInteractiveAsync(currentInput.TlgAgent, currentInput))
            .SkipLast(1) // Excluding the current BotCommand input
            .GetLatestRecordsUpTo(input => 
                input.InputType.Equals(TlgInputType.CommandMessage))
            .ToImmutableReadOnlyCollection();

        var previousWorkflow = workflowIdentifier.Identify(previousWorkflowInputHistory);

        return previousWorkflowInputHistory.Count > 0 && 
               previousWorkflow.IsSome && 
               !previousWorkflow.GetValueOrThrow().IsCompleted(previousWorkflowInputHistory);
    }

    private static bool IsCurrentInputFromOutOfScopeWorkflow(
        TlgInput currentInput, IReadOnlyCollection<TlgInput> activeWorkflowInputHistory) => 
        activeWorkflowInputHistory.GetLastBotCommand().Match(
            lastBotCommand => 
            {
                var activeWorkflowStartMessageId = lastBotCommand.Details.TlgMessageId;
                
                return currentInput.Details.TlgMessageId < activeWorkflowStartMessageId;
            },
            () => false);

    private static async Task<Result<WorkflowResponse>>
        GetResponseFromActiveWorkflowAsync(
            Option<IWorkflow> activeWorkflow,
            TlgInput currentInput)
    {
        return await activeWorkflow.Match(
            wf => 
                wf.GetResponseAsync(currentInput),
            () => 
                Task.FromResult(Result<WorkflowResponse>
                    .FromSuccess(new WorkflowResponse
                        ([new OutputDto 
                        { 
                            Text = Ui("My placeholder answer for lack of a workflow handling your input."), 
                        }], Option<string>.None()))));
    }

    private IReadOnlyCollection<OutputDto> ResolveResponseResultIntoOutputs(
        Result<WorkflowResponse> responseResult,
        List<OutputDto> outputBuilder,
        Option<IWorkflow> activeWorkflow,
        TlgInput currentInput)
    {
        return responseResult.Match(
            r => 
            {
                outputBuilder.AddRange(r.Output);
                        
                return 
                    outputBuilder
                        .ToImmutableReadOnlyCollection();
            },
            error =>
            {
                logger.LogWarning($"""
                                   The workflow '{activeWorkflow.GetValueOrDefault().GetType()}' has returned
                                   this Error Result: '{error}'. Next, the corresponding input parameters.
                                   UserId: {currentInput.TlgAgent.UserId}; ChatId: {currentInput.TlgAgent.ChatId}; 
                                   InputType: {currentInput.InputType}; InteractionMode: {currentInput.TlgAgent.Mode};
                                   Date: {currentInput.Details.TlgDate}; 
                                   For more details of input, check database!
                                   """);
                        
                return 
                    [new OutputDto { Text = error }];
            }
        );
    }
}
