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
    public async Task IdentifyAsync_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var tlgAgentWithoutRole = new TlgAgent(2468L, 13563897L, InteractionMode.Operations);
        var inputFromUnauthenticatedUser = utils.GetValidTlgInputTextMessage(
            tlgAgentWithoutRole.UserId, tlgAgentWithoutRole.ChatId);
    
        var workflow = await workflowIdentifier
            .IdentifyAsync(new List<TlgInput>{ inputFromUnauthenticatedUser }.ToImmutableReadOnlyCollection());
        
        Assert.True(workflow.GetValueOrThrow() is UserAuthWorkflow);
    }

    [Fact]
    public async Task IdentifyAsync_ReturnsLanguageSettingWorkflow_OnCorrespondingBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, InteractionMode.Operations);
        var inputWithSettingsBotCommand = utils.GetValidTlgInputCommandMessage(
            InteractionMode.Operations, (int)OperationsBotCommands.Settings,
            tlgAgent.UserId, tlgAgent.ChatId);
        
        var workflow = await workflowIdentifier
            .IdentifyAsync(new List<TlgInput>{ inputWithSettingsBotCommand }.ToImmutableReadOnlyCollection());
        
        Assert.True(workflow.GetValueOrThrow() is LanguageSettingWorkflow);
    }

    [Fact]
    public async Task IdentifyAsync_ReturnsNone_WhenCurrentInputsFromTlgAgent_WithoutBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var workflow = await workflowIdentifier
            .IdentifyAsync(new List<TlgInput>
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
