using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> currentInput);
}

internal class InputProcessor(
        InteractionMode interactionMode,
        // ToDo: Canddiate for a Func<TlgInput, IWorkflow> delegate!!?? 
        IWorkflowIdentifier workflowIdentifier,
        ITlgInputsRepository inputsRepo,
        ILogicUtils logicUtils,
        ILogger<InputProcessor> logger) 
    : IInputProcessor
{
    public async Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> currentInput)
    {
        return await currentInput.Match(
            async input =>
            {
                await inputsRepo.AddAsync(input);
                
                // ToDo: Probably here, add branching for InputType: Location vs. not Location... 
                // A Location update is not part of any workflow, it needs separate logic to handle location updates!

                // ToDo: remove this as soon as Repo handles caching
                await logicUtils.InitAsync();
                
                var outputBuilder = new List<OutputDto>();
                
                if (await IsInputInterruptingPreviousWorkflowAsync(input))
                {
                    outputBuilder.Add(new OutputDto
                        {
                            Text = Ui("FYI: you interrupted the previous workflow before its completion or " +
                                      "successful submission.")
                        });
                }

                var recentHistory = 
                    await logicUtils.GetInputsForCurrentWorkflow(input.TlgAgent);
                
                if (IsCurrentInputInOutOfScopeWorkflow(input, recentHistory))
                {
                    return [ new OutputDto 
                        {
                            Text = Ui("The previous workflow was completed, so your last message will be ignored.") 
                        }
                    ];
                }

                var activeWorkflow = workflowIdentifier.Identify(recentHistory);
                
                var nextWorkflowStepResult = await activeWorkflow.Match(
                    wf => wf.GetNextOutputAsync(input),
                    () => Task.FromResult(Result<IReadOnlyCollection<OutputDto>>.FromSuccess(
                    [ new OutputDto
                        {
                            Text = Ui("My placeholder answer for lack of a workflow handling your input."),
                        }
                    ])));

                return nextWorkflowStepResult.Match(
                    outputs =>
                    {
                        outputBuilder.AddRange(outputs);
                        return outputBuilder.ToImmutableReadOnlyCollection();
                    },
                    error =>
                    {
                        logger.LogWarning($"""
                                           The workflow '{activeWorkflow.GetValueOrDefault().GetType()}' has returned
                                           this Error Result: '{error}'. Next, the corresponding input parameters.
                                           UserId: {input.TlgAgent.UserId}; ChatId: {input.TlgAgent.ChatId}; 
                                           InputType: {input.InputType}; InteractionMode: {interactionMode};
                                           Date: {input.Details.TlgDate}; 
                                           For more details of input, check database!
                                           """);
                        
                        return [ new OutputDto { Text = error } ];
                    }
                );
            },
            // This error was already logged at its source, in ToModelConverter
            error => Task.FromResult<IReadOnlyCollection<OutputDto>>(
                [ new OutputDto { Text = error } ]));
    }

    private static bool IsCurrentInputInOutOfScopeWorkflow(
        TlgInput currentInput, IReadOnlyCollection<TlgInput> recentHistory) => 
        ILogicUtils.GetLastBotCommand(recentHistory).Match(
            lastBotCommand => 
            {
                var newWorkflowStartMessageId = lastBotCommand.Details.TlgMessageId;
                return currentInput.Details.TlgMessageId < newWorkflowStartMessageId;
            },
            () => false);

    private async Task<bool> IsInputInterruptingPreviousWorkflowAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CommandMessage)
            return false;
        
        var historyExcludingCurrentBotCommandInput = 
            (await logicUtils.GetAllInputsOfTlgAgentInCurrentRoleAsync(currentInput.TlgAgent))
            .SkipLast(1)
            .ToImmutableReadOnlyCollection();

        var previousWorkflow = workflowIdentifier.Identify(historyExcludingCurrentBotCommandInput);

        return previousWorkflow.IsSome && 
               !previousWorkflow.GetValueOrThrow().IsCompleted(historyExcludingCurrentBotCommandInput);
    }
}
