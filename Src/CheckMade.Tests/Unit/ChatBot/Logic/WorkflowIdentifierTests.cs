using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
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
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var tlgAgentWithoutRole = UserId02_ChatId03_Operations;
        var inputFromUnauthenticatedUser = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgentWithoutRole.UserId, tlgAgentWithoutRole.ChatId,
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
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var inputWithSettingsBotCommand = inputGenerator.GetValidTlgInputCommandMessage(
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
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var inputWithNewSubmissionBotCommand = inputGenerator.GetValidTlgInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.NewSubmission);
        
        var workflow = await workflowIdentifier
            .IdentifyAsync([inputWithNewSubmissionBotCommand]);
        
        Assert.True(
            workflow.GetValueOrThrow() is NewSubmissionWorkflow);
    }
    
    [Fact]
    public async Task Identify_ReturnsNone_WhenCurrentInputsFromTlgAgent_WithoutBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var workflow = await workflowIdentifier
            .IdentifyAsync([
                inputGenerator.GetValidTlgInputTextMessage(),
                inputGenerator.GetValidTlgInputTextMessageWithAttachment(TlgAttachmentType.Photo),
                // This could be in response to an out-of-scope message in the history e.g. in another Role!
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                inputGenerator.GetValidTlgInputTextMessage()
            ]);
        
        Assert.True(
            workflow.IsNone);
    }
}
