using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;
using CheckMade.Common.DomainModel.ChatBot;
using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.DomainModel.Core;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.DomainModel.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
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
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();

        var confirmLogoutCommand = inputGenerator.GetValidTlgInputCallbackQueryForControlPrompts(
            ControlPrompts.Yes);

        var boundRole = TestRepositoryUtils.GetNewRoleBind(
            SanitaryEngineer_DanielEn_X2024,
            PrivateBotChat_Operations);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                // Decoys
                inputGenerator.GetValidTlgInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidTlgInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Logout,
                    resultantWorkflowState: new ResultantWorkflowState(
                        glossary.GetId(typeof(LogoutWorkflow)),
                        glossary.GetId(typeof(ILogoutWorkflowConfirm))))
            ],
            roleBindings: [boundRole]);
        var mockRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
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
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        var tlgAgentOperations = PrivateBotChat_Operations;
        var tlgAgentComms = PrivateBotChat_Communications;
        var tlgAgentNotif = PrivateBotChat_Notifications;
        var boundRole = SanitaryEngineer_DanielEn_X2024; 
        
        var confirmLogoutCommand = inputGenerator.GetValidTlgInputCallbackQueryForControlPrompts(
            ControlPrompts.Yes);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidTlgInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Logout,
                    resultantWorkflowState: new ResultantWorkflowState(
                        glossary.GetId(typeof(LogoutWorkflow)),
                        glossary.GetId(typeof(ILogoutWorkflowConfirm))))
            ],
            roleBindings: new List<TlgAgentRoleBind>
            {
                // Relevant
                new(boundRole, tlgAgentOperations,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                new(boundRole, tlgAgentComms,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                new(boundRole, tlgAgentNotif,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                // Decoys
                new(boundRole, tlgAgentOperations,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None(), DbRecordStatus.SoftDeleted),
                new(SanitaryTeamLead_DanielDe_X2024, tlgAgentOperations,
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None()),
                new(boundRole, new TlgAgent(UserId02, ChatId04, Communications),
                    DateTimeOffset.UtcNow, Option<DateTimeOffset>.None())
            });
        var mockTlgAgentRoleBindingsForAllModes =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<LogoutWorkflow>();
        
        var expectedBindingsUpdated = 
            (await mockTlgAgentRoleBindingsForAllModes.Object.GetAllActiveAsync())
            .Where(tarb => 
                tarb.TlgAgent.UserId.Equals(tlgAgentOperations.UserId) &&
                tarb.TlgAgent.ChatId.Equals(tlgAgentOperations.ChatId) &&
                tarb.Role.Equals(boundRole))
            .ToList();
        
        List<TlgAgentRoleBind> actualTlgAgentRoleBindingsUpdated = [];
        mockTlgAgentRoleBindingsForAllModes
            .Setup(static x => x.UpdateStatusAsync(
                It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>(), DbRecordStatus.Historic))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>, DbRecordStatus>(
                (tlgAgentRoleBinds, newStatus) => 
                {
                    actualTlgAgentRoleBindingsUpdated = tlgAgentRoleBinds
                        .Select(tarb => tarb with
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
                expectedBindingsUpdated[i].TlgAgent,
                actualTlgAgentRoleBindingsUpdated[i].TlgAgent);
            Assert.Equivalent(
                expectedBindingsUpdated[i].Role,
                actualTlgAgentRoleBindingsUpdated[i].Role);
            Assert.Equal(
                DbRecordStatus.Historic, 
                actualTlgAgentRoleBindingsUpdated[i].Status);
        }
    }

    [Fact]
    public async Task GetResponseAsync_ConfirmsAbortion_AfterUserAbortsLogout()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        var abortLogoutCommand = inputGenerator.GetValidTlgInputCallbackQueryForControlPrompts(
            ControlPrompts.No);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                // Decoys
                inputGenerator.GetValidTlgInputCommandMessage(
                    Operations,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidTlgInputCommandMessage(
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