using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class LogoutWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetNextOutputAsync_LogsOutAndReturnsConfirmation_AfterUserConfirmsLogoutIntention()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var confirmLogoutCommand = utils.GetValidTlgInputCallbackQueryForControlPrompts(
            ControlPrompts.Yes,
            tlgAgent.UserId, tlgAgent.ChatId);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                // Decoys
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Settings,
                    tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                // Relevant
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Logout,
                    tlgAgent.UserId, tlgAgent.ChatId),
                confirmLogoutCommand
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();
        var workflow = _services.GetRequiredService<ILogoutWorkflow>();
        const string expectedMessage = "💨 Logged out.";
        var expectedBindUpdated = (await mockRoleBindingsRepo.Object.GetAllAsync())
            .First(arb => arb.TlgAgent == tlgAgent &&
                          arb.Status == DbRecordStatus.Active);
        
        var actualOutput = await workflow.GetNextOutputAsync(confirmLogoutCommand);
        
        Assert.Equal(expectedMessage, GetFirstRawEnglish(actualOutput));
        
        mockRoleBindingsRepo.Verify(x => x.UpdateStatusAsync(
                It.Is<IReadOnlyCollection<TlgAgentRoleBind>>(collection => 
                    collection.Any(bind => bind.Equals(expectedBindUpdated))),
                DbRecordStatus.Historic),
            Times.Once());
    }

    [Fact]
    public async Task GetNextOutputAsync_LogsOutFromAllModes_WhenLoggingOutInPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        var utils = _services.GetRequiredService<ITestUtils>();
        
        const long privateChatUserAndChatId = TestUserId_01;
        var tlgAgentOperations = new TlgAgent(privateChatUserAndChatId, privateChatUserAndChatId, Operations);
        var tlgAgentComms = new TlgAgent(privateChatUserAndChatId, privateChatUserAndChatId, Communications);
        var tlgAgentNotif = new TlgAgent(privateChatUserAndChatId, privateChatUserAndChatId, Notifications);
        var boundRole = SanitaryOpsEngineer1; 
        
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        var confirmLogoutCommand = utils.GetValidTlgInputCallbackQueryForControlPrompts(
            ControlPrompts.Yes,
            tlgAgentOperations.UserId, tlgAgentOperations.ChatId);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgentOperations))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Logout,
                    tlgAgentOperations.UserId, tlgAgentOperations.ChatId),
                confirmLogoutCommand
            });

        var mockTlgAgentRoleBindingsForAllModes = new Mock<ITlgAgentRoleBindingsRepository>();

        mockTlgAgentRoleBindingsForAllModes
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<TlgAgentRoleBind>
            {
                // Relevant
                new TlgAgentRoleBind(boundRole, tlgAgentOperations,
                    DateTime.UtcNow, Option<DateTime>.None()),
                new TlgAgentRoleBind(boundRole, tlgAgentComms,
                    DateTime.UtcNow, Option<DateTime>.None()),
                new TlgAgentRoleBind(boundRole, tlgAgentNotif,
                    DateTime.UtcNow, Option<DateTime>.None()),
                // Decoys
                new TlgAgentRoleBind(boundRole, tlgAgentOperations,
                    DateTime.UtcNow, Option<DateTime>.None(), DbRecordStatus.SoftDeleted),
                new TlgAgentRoleBind(SanitaryOpsCleanLead1German, tlgAgentOperations,
                    DateTime.UtcNow, Option<DateTime>.None()),
                new TlgAgentRoleBind(boundRole, new TlgAgent(TestUserId_02, TestChatId_04, Communications),
                    DateTime.UtcNow, Option<DateTime>.None())
            });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        serviceCollection.AddScoped<ITlgAgentRoleBindingsRepository>(_ => mockTlgAgentRoleBindingsForAllModes.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILogoutWorkflow>();
        
        var expectedBindingsUpdated = 
            (await mockTlgAgentRoleBindingsForAllModes.Object.GetAllAsync())
            .Where(arb => 
                arb.TlgAgent.UserId == tlgAgentOperations.UserId &&
                arb.TlgAgent.ChatId == tlgAgentOperations.ChatId &&
                arb.Role == boundRole && 
                arb.Status == DbRecordStatus.Active)
            .ToImmutableReadOnlyList();
        
        var actualTlgAgentRoleBindingsUpdated = new List<TlgAgentRoleBind>();
        mockTlgAgentRoleBindingsForAllModes
            .Setup(x => x.UpdateStatusAsync(
                It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>(), DbRecordStatus.Historic))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>, DbRecordStatus>(
                (tlgAgentRoleBinds, newStatus) => 
                {
                    actualTlgAgentRoleBindingsUpdated = tlgAgentRoleBinds
                        .Select(arb => arb with
                        {
                            DeactivationDate = DateTime.UtcNow,
                            Status = newStatus
                        })
                        .ToList();
                });
        
        await workflow.GetNextOutputAsync(confirmLogoutCommand);

        for (var i = 0; i < expectedBindingsUpdated.Count; i++)
        {
            Assert.Equivalent(expectedBindingsUpdated[i].TlgAgent, actualTlgAgentRoleBindingsUpdated[i].TlgAgent);
            Assert.Equivalent(expectedBindingsUpdated[i].Role, actualTlgAgentRoleBindingsUpdated[i].Role);
            Assert.True(actualTlgAgentRoleBindingsUpdated[i].Status == DbRecordStatus.Historic);
        }
    }

    [Fact]
    public async Task GetNextOutputAsync_ConfirmsAbortion_AfterUserAbortsLogout()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var abortLogoutCommand = utils.GetValidTlgInputCallbackQueryForControlPrompts(
            ControlPrompts.No,
            tlgAgent.UserId, tlgAgent.ChatId);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                // Decoys
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Settings,
                    tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                // Relevant
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Logout,
                    tlgAgent.UserId, tlgAgent.ChatId),
                abortLogoutCommand
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILogoutWorkflow>();
        var expectedMessage1 = "Logout aborted.\n"; 
        
        var actualOutput = await workflow.GetNextOutputAsync(abortLogoutCommand);
        
        Assert.Equal(expectedMessage1, GetFirstRawEnglish(actualOutput));
        Assert.Equivalent(
            IInputProcessor.SeeValidBotCommandsInstruction.RawEnglishText, 
            actualOutput.GetValueOrThrow().First().Text.GetValueOrThrow().Concatenations.Last()!.RawEnglishText);
    }
}