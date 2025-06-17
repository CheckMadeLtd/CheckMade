using CheckMade.Bot.Workflows;
using CheckMade.Bot.Workflows.Workflows.Global.LanguageSetting;
using CheckMade.Bot.Workflows.Workflows.Global.UserAuth;
using CheckMade.Bot.Workflows.Workflows.Operations.NewSubmission;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using General.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public sealed class WorkflowIdentifierTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task Identify_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var agentWithoutRole = UserId02_ChatId03_Operations;
        var inputFromUnauthenticatedUser = inputGenerator.GetValidInputTextMessage(
            agentWithoutRole.UserId, agentWithoutRole.ChatId,
            roleSetting: TestOriginatorRoleSetting.None);

        var workflow = await workflowIdentifier
            .IdentifyAsync([inputFromUnauthenticatedUser]);
        
        Assert.True(
            workflow.GetValueOrThrow() is UserAuthWorkflow);
    }

    [Fact]
    public async Task Identify_ReturnsLanguageSettingWorkflow_OnCorrespondingBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var inputWithSettingsBotCommand = inputGenerator.GetValidInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings);
        
        var workflow = await workflowIdentifier
            .IdentifyAsync([inputWithSettingsBotCommand]);
        
        Assert.True(
            workflow.GetValueOrThrow() is LanguageSettingWorkflow);
    }

    [Fact]
    public async Task Identify_ReturnsNewSubmissionWorkflow_OnCorrespondingBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var inputWithNewSubmissionBotCommand = inputGenerator.GetValidInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.NewSubmission);
        
        var workflow = await workflowIdentifier
            .IdentifyAsync([inputWithNewSubmissionBotCommand]);
        
        Assert.True(
            workflow.GetValueOrThrow() is NewSubmissionWorkflow);
    }
    
    [Fact]
    public async Task Identify_ReturnsNone_WhenCurrentInputsFromAgent_WithoutBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var workflow = await workflowIdentifier
            .IdentifyAsync([
                inputGenerator.GetValidInputTextMessage(),
                inputGenerator.GetValidInputTextMessageWithAttachment(AttachmentType.Photo),
                // This could be in response to an out-of-scope message in the history e.g. in another Role!
                inputGenerator.GetValidInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                inputGenerator.GetValidInputTextMessage()
            ]);
        
        Assert.True(
            workflow.IsNone);
    }
}
