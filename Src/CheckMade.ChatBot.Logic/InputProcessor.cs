using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput);
}

internal class InputProcessor(
#pragma warning disable CS9113 // Parameter is unread.
    InteractionMode interactionMode,
#pragma warning restore CS9113 // Parameter is unread.
    IWorkflowIdentifier workflowIdentifier,
        ITlgInputRepository inputRepo) 
    : IInputProcessor
{
    public async Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput)
    {
        return await tlgInput.Match(
            async input =>
            {
                await inputRepo.AddAsync(input);
                
                var currentWorkflow = await workflowIdentifier.IdentifyAsync(input);

                var nextWorkflowStepResult = await currentWorkflow.Match(
                    wf => wf.GetNextOutputAsync(input),
                    () => Task.FromResult(Result<IReadOnlyList<OutputDto>>.FromSuccess(new List<OutputDto>())));

                return nextWorkflowStepResult.Match(
                    outputs => outputs,
                    error => [new OutputDto { Text = error }] // ToDo: this should also log a warning!
                );
            },
            // ToDo: this should also log a warning!
            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
        );
    }
}
