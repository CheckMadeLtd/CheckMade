using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;

// ToDo: Remove again once I use inputRepo for more sophisticated workflow identification.
#pragma warning disable CS9113 // Parameter is unread.

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Task<Option<IWorkflow>> IdentifyAsync(TlgInput input);
}

internal class WorkflowIdentifier(
        ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortRoleRepository portRoleRepo,
        IWorkflowUtils workflowUtils) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(TlgInput input)
    {
        var inputPort = new TlgClientPort(input.UserId, input.ChatId, input.InteractionMode);

        if (!await IsUserAuthenticated(inputPort, input.InteractionMode, portRoleRepo))
        {
            return await UserAuthWorkflow.CreateAsync(roleRepo, portRoleRepo, workflowUtils);
        }

        // the settings BotCommand code is the same across all InteractionModes
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (input.Details.BotCommandEnumCode == (int)OperationsBotCommands.Settings)
        {
            return new LanguageSettingWorkflow(portRoleRepo);
        }
        
        return Option<IWorkflow>.None();
    }
    
    private static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, InteractionMode mode, ITlgClientPortRoleRepository portRoleRepo)
    {
        IReadOnlyList<TlgClientPortRole> tlgClientPortRoles =
            (await portRoleRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortRoles
                   .FirstOrDefault(cpr => 
                       cpr.ClientPort.ChatId == inputPort.ChatId &&
                       cpr.ClientPort.Mode == mode &&
                       cpr.Status == DbRecordStatus.Active) 
               != null;
    }
}