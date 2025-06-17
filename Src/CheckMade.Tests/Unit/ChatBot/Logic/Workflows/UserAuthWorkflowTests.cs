using CheckMade.ChatBot.Logic.Workflows.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Global.UserAuth.States;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using InputValidator = CheckMade.Common.Utils.Validators.InputValidator;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public sealed class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetResponseAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var glossary = _services.GetRequiredService<IDomainGlossary>();

        List<Input> inputHistory = 
        [
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                Start.CommandCode,
                agent.UserId,
                agent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryEngineer_DanielEn_X2024]);
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var nonExistingTokenInput = inputGenerator.GetValidInputTextMessage(
            text: InputValidator.GetTokenFormatExample());
        
        var actualResponses = 
            await workflow
                .GetResponseAsync(nonExistingTokenInput);
        
        Assert.Equal(
            "This is an unknown token. Try again...",
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
    }

    [Fact]
    public async Task GetResponseAsync_ReturnsWarning_AndDeactivatesPreExisting_WhenTokenAlreadyHasActiveAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<Input> inputHistory = 
        [
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                Start.CommandCode,
                agent.UserId,
                agent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var preExistingActiveAgentRoleBind = 
            TestRepositoryUtils.GetNewRoleBind(
                SanitaryAdmin_DanielEn_X2024,
                agent);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roleBindings: [preExistingActiveAgentRoleBind]);
        var mockAgentRoleBindingsRepo = 
            (Mock<IAgentRoleBindingsRepository>)container.Mocks[typeof(IAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var inputTokenWithPreExistingActiveAgentRoleBind = 
            inputGenerator.GetValidInputTextMessage(
                text: SanitaryAdmin_DanielEn_X2024.Token);
        
        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat.
                                       This will be the new {0} chat where you receive messages at {1}, in your role as: 
                                       """;
        
        var actualResponses = 
            await workflow
                .GetResponseAsync(inputTokenWithPreExistingActiveAgentRoleBind);
        
        Assert.Equal(
            expectedWarning,
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
        
        mockAgentRoleBindingsRepo.Verify(x => 
            x.UpdateStatusAsync(
                preExistingActiveAgentRoleBind,
                DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetResponseAsync_CreatesRoleBind_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = UserId03_ChatId06_Operations;
        var roleForAuth = SanitaryEngineer_DanielEn_X2024;
        var glossary = _services.GetRequiredService<IDomainGlossary>();

        List<Input> inputHistory = 
        [
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                Start.CommandCode,
                agent.UserId,
                agent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryEngineer_DanielEn_X2024]);
        var mockRoleBindingsRepo =
            (Mock<IAgentRoleBindingsRepository>)container.Mocks[typeof(IAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var inputValidToken = inputGenerator.GetValidInputTextMessage(
            userId: agent.UserId,
            chatId: agent.ChatId,
            text: roleForAuth.Token,
            resultantWorkflowState: new ResultantWorkflowState(
                glossary.GetId(typeof(UserAuthWorkflow)),
                glossary.GetId(typeof(IUserAuthWorkflowAuthenticated))));

        const string expectedConfirmation = "{0}, you have successfully authenticated at live-event {1} in your role as: ";
        
        var expectedAgentRoleBindAdded = new AgentRoleBind(
            roleForAuth,
            agent,
            DateTimeOffset.UtcNow,
            Option<DateTimeOffset>.None());
        
        List<AgentRoleBind> actualAgentRoleBindAdded = []; 
        mockRoleBindingsRepo
            .Setup(static x => 
                x.AddAsync(It.IsAny<IReadOnlyCollection<AgentRoleBind>>()))
            .Callback<IReadOnlyCollection<AgentRoleBind>>(agentRoleBind => 
                actualAgentRoleBindAdded = agentRoleBind.ToList());
        
        var actualResponses = await workflow.GetResponseAsync(inputValidToken);
        
        Assert.Equal(
            expectedConfirmation,
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
        Assert.Equivalent(
            expectedAgentRoleBindAdded.Role,
            actualAgentRoleBindAdded[0].Role);
        Assert.Equivalent(
            expectedAgentRoleBindAdded.Agent,
            actualAgentRoleBindAdded[0].Agent);
        Assert.Equivalent(
            expectedAgentRoleBindAdded.Status,
            actualAgentRoleBindAdded[0].Status);
    }

    [Fact]
    public async Task GetResponseAsync_CreatesRoleBindingsForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var roleForAuth = SanitaryInspector_DanielDe_X2024;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<Input> inputHistory = 
        [
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                Start.CommandCode,
                agent.UserId,
                agent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryInspector_DanielDe_X2024]);
        var mockAgentRoleBindingsRepo =
            (Mock<IAgentRoleBindingsRepository>)container.Mocks[typeof(IAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var inputValidToken = inputGenerator.GetValidInputTextMessage(
            userId: agent.UserId,
            chatId: agent.ChatId,
            text: roleForAuth.Token);

        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        var expectedAgentRoleBindingsAdded = allModes.Select(im => 
                new AgentRoleBind(
                    roleForAuth,
                    agent with { Mode = im },
                    DateTimeOffset.UtcNow,
                    Option<DateTimeOffset>.None()))
            .ToList();

        List<AgentRoleBind> actualAgentRoleBindingsAdded = [];
        mockAgentRoleBindingsRepo
            .Setup(static x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<AgentRoleBind>>()))
            .Callback<IReadOnlyCollection<AgentRoleBind>>(
                agentRoleBindings => actualAgentRoleBindingsAdded = agentRoleBindings.ToList());

        await workflow.GetResponseAsync(inputValidToken);
        
        mockAgentRoleBindingsRepo.Verify(static x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<AgentRoleBind>>()));

        for (var i = 0; i < expectedAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(
                expectedAgentRoleBindingsAdded[i].Agent,
                actualAgentRoleBindingsAdded[i].Agent);
            Assert.Equivalent(
                expectedAgentRoleBindingsAdded[i].Role,
                actualAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(
                expectedAgentRoleBindingsAdded[i].Status,
                actualAgentRoleBindingsAdded[i].Status);
        }
    }
    
    [Fact]
    public async Task GetResponseAsync_CreatesRoleBindingsForMissingModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var roleForAuth = SanitaryEngineer_DanielEn_X2024;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<Input> inputHistory = 
        [
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                Start.CommandCode,
                agent.UserId,
                agent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryEngineer_DanielEn_X2024],
            roleBindings:
            [
                TestRepositoryUtils.GetNewRoleBind(
                    SanitaryEngineer_DanielEn_X2024, agent with { Mode = Communications })
            ]);
        var mockAgentRoleBindingsRepo =
            (Mock<IAgentRoleBindingsRepository>)container.Mocks[typeof(IAgentRoleBindingsRepository)];

        var inputValidToken = inputGenerator.GetValidInputTextMessage(
            userId: agent.UserId,
            chatId: agent.ChatId,
            text: roleForAuth.Token);

        List<AgentRoleBind> expectedAgentRoleBindingsAdded =
        [
            // Adds missing bind for Operations Mode
            new(roleForAuth,
                agent,
                DateTimeOffset.UtcNow,
                Option<DateTimeOffset>.None()),

            // Adds missing bind for Notifications Mode
            new(roleForAuth,
                agent with { Mode = Notifications },
                DateTimeOffset.UtcNow,
                Option<DateTimeOffset>.None()),
        ];

        List<AgentRoleBind> actualAgentRoleBindingsAdded = [];
        mockAgentRoleBindingsRepo
            .Setup(static x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<AgentRoleBind>>()))
            .Callback<IReadOnlyCollection<AgentRoleBind>>(
                agentRoleBindings => actualAgentRoleBindingsAdded = agentRoleBindings.ToList());
        
        var workflow = services.GetRequiredService<UserAuthWorkflow>();

        await workflow.GetResponseAsync(inputValidToken);
        
        mockAgentRoleBindingsRepo.Verify(static x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<AgentRoleBind>>()));

        for (var i = 0; i < expectedAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(
                expectedAgentRoleBindingsAdded[i].Agent,
                actualAgentRoleBindingsAdded[i].Agent);
            Assert.Equivalent(
                expectedAgentRoleBindingsAdded[i].Role, 
                actualAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(
                expectedAgentRoleBindingsAdded[i].Status,
                actualAgentRoleBindingsAdded[i].Status);
        }
    }
    
    [Theory]
    [InlineData("5JFUX")]
    [InlineData(" ")]
    [InlineData(" some text with trailing spaces and \n line break ")]
    [InlineData("")]
    public async Task GetResponseAsync_ReturnsCorrectErrorMessage_WhenBadFormatTokenEntered(string badToken)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<Input> inputHistory = 
        [
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                Start.CommandCode,
                agent.UserId,
                agent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryEngineer_DanielEn_X2024],
            roleBindings:
            [
                TestRepositoryUtils.GetNewRoleBind(
                    SanitaryEngineer_DanielEn_X2024, agent with { Mode = Communications })
            ]);
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var badTokenInput = inputGenerator.GetValidInputTextMessage(text: badToken);
        
        var actualResponses = await workflow.GetResponseAsync(badTokenInput);
        
        Assert.Equal(
            "Bad token format! Try again...",
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
    }
}