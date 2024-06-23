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

    public Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput);
}

internal class InputProcessor(
        InteractionMode interactionMode,
        // ToDo: Canddiate for a Func<TlgInput, IWorkflow> delegate!!?? 
        IWorkflowIdentifier workflowIdentifier,
        ITlgInputsRepository inputsRepo,
        ILogger<InputProcessor> logger) 
    : IInputProcessor
{
    public async Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput)
    {
        return await tlgInput.Match(
            async input =>
            {
                await inputsRepo.AddAsync(input);
                
                // ToDo: Probably here, add branching for InputType: Location vs. not Location... 
                // A Location update is not part of any workflow, it needs separate logic to handle location updates!
                
                
                
                var currentWorkflow = await workflowIdentifier.IdentifyAsync(input);

                var nextWorkflowStepResult = await currentWorkflow.Match(
                    wf => wf.GetNextOutputAsync(input),
                    () => Task.FromResult(Result<IReadOnlyCollection<OutputDto>>.FromSuccess(
                        new List<OutputDto>
                        {
                            new()
                            {
                                Text = Ui("My placeholder answer for lack of a workflow handling your input."),
                            }
                        })));

                return nextWorkflowStepResult.Match(
                    outputs => outputs,
                    error =>
                    {
                        logger.LogWarning($"""
                                           The workflow '{currentWorkflow.GetValueOrDefault().GetType()}' has returned
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
}
