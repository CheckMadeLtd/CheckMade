using CheckMade.ChatBot.Logic.Workflows.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Global.Logout.States;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public sealed class LogoutWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetResponseAsync_LogsOutAndReturnsConfirmation_AfterUserConfirmsLogoutIntention()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();

        var confirmLogoutCommand = inputGenerator.GetValidInputCallbackQueryForControlPrompts(
            ControlPrompts.Yes);

        var boundRole = TestRepositoryUtils.GetNewRoleBind(
            SanitaryEngineer_DanielEn_X2024,
            PrivateBotChat_Operations);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                // Decoys
                inputGenerator.GetValidInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Logout,
                    resultantWorkflowState: new ResultantWorkflowState(
                        glossary.GetId(typeof(LogoutWorkflow)),
                        glossary.GetId(typeof(ILogoutWorkflowConfirm))))
            ],
            roleBindings: [boundRole]);
        var mockRoleBindingsRepo =
            (Mock<IAgentRoleBindingsRepository>)container.Mocks[typeof(IAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<LogoutWorkflow>();
        
        const string expectedMessage = "💨 Logged out.";
        
        var actualResponse = await workflow.GetResponseAsync(confirmLogoutCommand);
        
        Assert.Contains(
            expectedMessage, 
            actualResponse.GetValueOrThrow().Output.GetAllRawEnglish());
        
        mockRoleBindingsRepo.Verify(x => x.UpdateStatusAsync(
                new[] { boundRole },
                DbRecordStatus.Historic),
            Times.Once());
    }

    [Fact]
    public async Task GetResponseAsync_LogsOutFromAllModes_WhenLoggingOutInPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        var agentOperations = PrivateBotChat_Operations;
        var agentComms = PrivateBotChat_Communications;
        var agentNotif = PrivateBotChat_Notifications;
        var boundRole = SanitaryEngineer_DanielEn_X2024; 
        
        var confirmLogoutCommand = inputGenerator.GetValidInputCallbackQueryForControlPrompts(
            ControlPrompts.Yes);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Logout,
                    resultantWorkflowState: new ResultantWorkflowState(
                        glossary.GetId(typeof(LogoutWorkflow)),
                        glossary.GetId(typeof(ILogoutWorkflowConfirm))))
            ],
            roleBindings: new List<AgentRoleBind>
            {
                // Relevant
                new(boundRole, agentOperations,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                new(boundRole, agentComms,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                new(boundRole, agentNotif,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                // Decoys
                new(boundRole, agentOperations,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None(), DbRecordStatus.SoftDeleted),
                new(SanitaryTeamLead_DanielDe_X2024, agentOperations,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                new(boundRole, new Agent(UserId02, ChatId04, Communications),
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None())
            });
        var mockAgentRoleBindingsForAllModes =
            (Mock<IAgentRoleBindingsRepository>)container.Mocks[typeof(IAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<LogoutWorkflow>();
        
        var expectedBindingsUpdated = 
            (await mockAgentRoleBindingsForAllModes.Object.GetAllActiveAsync())
            .Where(arb => 
                arb.Agent.UserId.Equals(agentOperations.UserId) &&
                arb.Agent.ChatId.Equals(agentOperations.ChatId) &&
                arb.Role.Equals(boundRole))
            .ToList();
        
        List<AgentRoleBind> actualAgentRoleBindingsUpdated = [];
        mockAgentRoleBindingsForAllModes
            .Setup(static x => x.UpdateStatusAsync(
                It.IsAny<IReadOnlyCollection<AgentRoleBind>>(), DbRecordStatus.Historic))
            .Callback<IReadOnlyCollection<AgentRoleBind>, DbRecordStatus>(
                (agentRoleBinds, newStatus) => 
                {
                    actualAgentRoleBindingsUpdated = agentRoleBinds
                        .Select(arb => arb with
                        {
                            DeactivationDate = DateTimeOffset.UtcNow,
                            Status = newStatus
                        })
                        .ToList();
                });
        
        await workflow.GetResponseAsync(confirmLogoutCommand);

        for (var i = 0; i < expectedBindingsUpdated.Count; i++)
        {
            Assert.Equivalent(
                expectedBindingsUpdated[i].Agent,
                actualAgentRoleBindingsUpdated[i].Agent);
            Assert.Equivalent(
                expectedBindingsUpdated[i].Role,
                actualAgentRoleBindingsUpdated[i].Role);
            Assert.Equal(
                DbRecordStatus.Historic, 
                actualAgentRoleBindingsUpdated[i].Status);
        }
    }

    [Fact]
    public async Task GetResponseAsync_ConfirmsAbortion_AfterUserAbortsLogout()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        var abortLogoutCommand = inputGenerator.GetValidInputCallbackQueryForControlPrompts(
            ControlPrompts.No);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                // Decoys
                inputGenerator.GetValidInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Logout,
                    resultantWorkflowState: new ResultantWorkflowState(
                        glossary.GetId(typeof(LogoutWorkflow)),
                        glossary.GetId(typeof(ILogoutWorkflowConfirm))))
            ]);
        var workflow = services.GetRequiredService<LogoutWorkflow>();
        const string expectedMessage1 = "Logout aborted."; 
        
        var actualResponse = 
            await workflow.GetResponseAsync(abortLogoutCommand);
        
        Assert.Contains(
            expectedMessage1,
            actualResponse.GetValueOrThrow().Output.GetAllRawEnglish());
        
        Assert.Contains(
            IInputProcessor.SeeValidBotCommandsInstruction.RawEnglishText, 
            actualResponse.GetValueOrThrow().Output.GetAllRawEnglish());
    }
}