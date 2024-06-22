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
        await workflowUtils.InitAsync();
        
        if (!IsUserAuthenticated(input.TlgAgent))
        {
            return Option<IWorkflow>.Some(userAuthWorkflow);
        }

        var inputs = await workflowUtils.GetInputsForCurrentWorkflow(input.TlgAgent);
        var lastBotCommand = GetLastBotCommand(inputs);

        return lastBotCommand.Match(
            cmd => cmd.Details.BotCommandEnumCode.GetValueOrThrow() switch
            {
                // the settings BotCommand code is the same across all InteractionModes
                (int)OperationsBotCommands.Settings => Option<IWorkflow>.Some(languageSettingWorkflow),
                _ => Option<IWorkflow>.None()
            },
            Option<IWorkflow>.None);
    }
    
    private bool IsUserAuthenticated(TlgAgent inputTlgAgent) => 
        workflowUtils.GetAllTlgAgentRoles()
        .FirstOrDefault(arb => 
            arb.TlgAgent.ChatId == inputTlgAgent.ChatId && 
            arb.TlgAgent.Mode == inputTlgAgent.Mode && 
            arb.Status == DbRecordStatus.Active) 
        != null;

    private static Option<TlgInput> GetLastBotCommand(IReadOnlyCollection<TlgInput> inputs) =>
        inputs.LastOrDefault(i => i.Details.BotCommandEnumCode.IsSome)
        ?? Option<TlgInput>.None();
}