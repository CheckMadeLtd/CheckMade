using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Task<Option<IWorkflow>> IdentifyAsync(TlgInput input);
}

internal class WorkflowIdentifier(
        IWorkflowUtils workflowUtils,
        IUserAuthWorkflow userAuthWorkflow,
        ILanguageSettingWorkflow languageSettingWorkflow) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(TlgInput input)
    {
        if (!await IsUserAuthenticated(input.ClientPort))
        {
            return Option<IWorkflow>.Some(userAuthWorkflow);
        }

        // the settings BotCommand code is the same across all InteractionModes
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (input.Details.BotCommandEnumCode == (int)OperationsBotCommands.Settings)
        {
            return Option<IWorkflow>.Some(languageSettingWorkflow);
        }
        
        return Option<IWorkflow>.None();
    }
    
    private async Task<bool> IsUserAuthenticated(TlgClientPort inputPort) => 
        (await workflowUtils.GetAllClientPortRolesAsync())
        .FirstOrDefault(cpr => 
            cpr.ClientPort.ChatId == inputPort.ChatId && 
            cpr.ClientPort.Mode == inputPort.Mode && 
            cpr.Status == DbRecordStatus.Active) 
        != null;
}