using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic;

public class WorkflowIdentifierTests
{
    private ServiceProvider? _services;

    [Fact]
    public void Identify_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var tlgAgentWithoutRole = TlgAgent_HasOnly_HistoricRoleBind;
        var inputFromUnauthenticatedUser = utils.GetValidTlgInputTextMessage(
            tlgAgentWithoutRole.UserId, tlgAgentWithoutRole.ChatId,
            roleSetting: TestOriginatorRoleSetting.None);
    
        var workflow = workflowIdentifier
            .Identify(new List<TlgInput>{ inputFromUnauthenticatedUser }.ToImmutableReadOnlyCollection());
        
        Assert.True(workflow.GetValueOrThrow() is UserAuthWorkflow);
    }

    [Fact]
    public void Identify_ReturnsLanguageSettingWorkflow_OnCorrespondingBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var inputWithSettingsBotCommand = utils.GetValidTlgInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings);
        
        var workflow = workflowIdentifier
            .Identify(new List<TlgInput>{ inputWithSettingsBotCommand }.ToImmutableReadOnlyCollection());
        
        Assert.True(workflow.GetValueOrThrow() is LanguageSettingWorkflow);
    }

    [Fact]
    public void Identify_ReturnsNone_WhenCurrentInputsFromTlgAgent_WithoutBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var workflow = workflowIdentifier
            .Identify(new List<TlgInput>
                {
                    utils.GetValidTlgInputTextMessage(),
                    utils.GetValidTlgInputTextMessageWithAttachment(TlgAttachmentType.Photo),
                    // This could be in response to an out-of-scope message in the history e.g. in another Role!
                    utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                    utils.GetValidTlgInputTextMessage()
                });
        
        Assert.True(workflow.IsNone);
    }
}
