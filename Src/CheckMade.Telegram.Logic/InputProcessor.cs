using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Telegram.Logic.Workflows;

namespace CheckMade.Telegram.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput);

    // internal because outside of InputProcessor, only accessible to tests.
    internal Task<Option<IWorkflow>> IdentifyCurrentWorkflowAsync(TlgInput input);
}

internal class InputProcessor(
        InteractionMode interactionMode,    
        ITlgInputRepository inputRepo,
        ITlgClientPortToRoleMapRepository portToRoleMapRepo) 
    : IInputProcessor
{
    public async Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput)
    {
        return await tlgInput.Match(
            async input =>
            {
                await inputRepo.AddAsync(input);
                
                var currentWorkflow = await IdentifyCurrentWorkflowAsync(input);

                var nextWorkflowStepResult = await currentWorkflow.Match(
                    wf => wf.GetNextOutputAsync(input),
                    () => Task.FromResult(Result<IReadOnlyList<OutputDto>>.FromSuccess(new List<OutputDto>())));

                return nextWorkflowStepResult.Match(
                    outputs => outputs,
                    error => [new OutputDto { Text = error }]
                );
            },
            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
        );
    }

    public async Task<Option<IWorkflow>> IdentifyCurrentWorkflowAsync(TlgInput input)
    {
        var inputPort = new TlgClientPort(input.UserId, input.ChatId);

        if (!(await IsUserAuthenticated(inputPort, portToRoleMapRepo)))
        {
            return new UserAuthWorkflow(inputRepo);
        }
        
        return Option<IWorkflow>.None();
    }

    private static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, ITlgClientPortToRoleMapRepository mapRepo)
    {
        IReadOnlyList<TlgClientPortToRoleMap> tlgClientPortToRoleMap =
            (await mapRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortToRoleMap
                   .FirstOrDefault(map => map.ClientPort.ChatId == inputPort.ChatId &&
                                          map.Status == DbRecordStatus.Active) 
               != null;
    }
}
