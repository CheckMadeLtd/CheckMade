using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public class WorkflowIdentifierTests
{
    private ServiceProvider? _services;

    [Fact]
    public void Identify_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var tlgAgentWithoutRole = UserId02_ChatId03_Operations;
        var inputFromUnauthenticatedUser = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgentWithoutRole.UserId, tlgAgentWithoutRole.ChatId,
            roleSetting: TestOriginatorRoleSetting.None);
    
        var workflow = workflowIdentifier
            .Identify(new [] { inputFromUnauthenticatedUser }
                .ToImmutableReadOnlyCollection());
        
        Assert.True(
            workflow.GetValueOrThrow() is UserAuthWorkflow);
    }

    [Fact]
    public void Identify_ReturnsLanguageSettingWorkflow_OnCorrespondingBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var inputWithSettingsBotCommand = inputGenerator.GetValidTlgInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings);
        
        var workflow = workflowIdentifier
            .Identify(new [] { inputWithSettingsBotCommand }
                .ToImmutableReadOnlyCollection());
        
        Assert.True(
            workflow.GetValueOrThrow() is LanguageSettingWorkflow);
    }

    [Fact]
    public void Identify_ReturnsNone_WhenCurrentInputsFromTlgAgent_WithoutBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var workflow = workflowIdentifier
            .Identify(new []
                {
                    inputGenerator.GetValidTlgInputTextMessage(),
                    inputGenerator.GetValidTlgInputTextMessageWithAttachment(TlgAttachmentType.Photo),
                    // This could be in response to an out-of-scope message in the history e.g. in another Role!
                    inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                    inputGenerator.GetValidTlgInputTextMessage()
                });
        
        Assert.True(
            workflow.IsNone);
    }
}
