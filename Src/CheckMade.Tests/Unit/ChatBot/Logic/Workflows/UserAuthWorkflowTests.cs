using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth.States;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using InputValidator = CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public sealed class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetResponseAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var glossary = _services.GetRequiredService<IDomainGlossary>();

        List<TlgInput> inputHistory = 
        [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                TlgStart.CommandCode,
                tlgAgent.UserId,
                tlgAgent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryEngineer_DanielEn_X2024]);
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var nonExistingTokenInput = inputGenerator.GetValidTlgInputTextMessage(
            text: InputValidator.GetTokenFormatExample());
        
        var actualResponses = 
            await workflow
                .GetResponseAsync(nonExistingTokenInput);
        
        Assert.Equal(
            "This is an unknown token. Try again...",
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
    }

    [Fact]
    public async Task GetResponseAsync_ReturnsWarning_AndDeactivatesPreExisting_WhenTokenAlreadyHasActiveTlgAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<TlgInput> inputHistory = 
        [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                TlgStart.CommandCode,
                tlgAgent.UserId,
                tlgAgent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var preExistingActiveTlgAgentRoleBind = 
            TestRepositoryUtils.GetNewRoleBind(
                SanitaryAdmin_DanielEn_X2024,
                tlgAgent);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roleBindings: [preExistingActiveTlgAgentRoleBind]);
        var mockTlgAgentRoleBindingsRepo = 
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var inputTokenWithPreExistingActiveTlgAgentRoleBind = 
            inputGenerator.GetValidTlgInputTextMessage(
                text: SanitaryAdmin_DanielEn_X2024.Token);
        
        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat.
                                       This will be the new {0} chat where you receive messages at {1}, in your role as: 
                                       """;
        
        var actualResponses = 
            await workflow
                .GetResponseAsync(inputTokenWithPreExistingActiveTlgAgentRoleBind);
        
        Assert.Equal(
            expectedWarning,
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
        
        mockTlgAgentRoleBindingsRepo.Verify(x => 
            x.UpdateStatusAsync(
                preExistingActiveTlgAgentRoleBind,
                DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetResponseAsync_CreatesRoleBind_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = UserId03_ChatId06_Operations;
        var roleForAuth = SanitaryEngineer_DanielEn_X2024;
        var glossary = _services.GetRequiredService<IDomainGlossary>();

        List<TlgInput> inputHistory = 
        [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                TlgStart.CommandCode,
                tlgAgent.UserId,
                tlgAgent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryEngineer_DanielEn_X2024]);
        var mockRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token,
            resultantWorkflowState: new ResultantWorkflowState(
                glossary.GetId(typeof(UserAuthWorkflow)),
                glossary.GetId(typeof(IUserAuthWorkflowAuthenticated))));

        const string expectedConfirmation = "{0}, you have successfully authenticated at live-event {1} in your role as: ";
        
        var expectedTlgAgentRoleBindAdded = new TlgAgentRoleBind(
            roleForAuth,
            tlgAgent,
            DateTimeOffset.UtcNow,
            Option<DateTimeOffset>.None());
        
        List<TlgAgentRoleBind> actualTlgAgentRoleBindAdded = []; 
        mockRoleBindingsRepo
            .Setup(static x => 
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(tlgAgentRole => 
                actualTlgAgentRoleBindAdded = tlgAgentRole.ToList());
        
        var actualResponses = await workflow.GetResponseAsync(inputValidToken);
        
        Assert.Equal(
            expectedConfirmation,
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
        Assert.Equivalent(
            expectedTlgAgentRoleBindAdded.Role,
            actualTlgAgentRoleBindAdded[0].Role);
        Assert.Equivalent(
            expectedTlgAgentRoleBindAdded.TlgAgent,
            actualTlgAgentRoleBindAdded[0].TlgAgent);
        Assert.Equivalent(
            expectedTlgAgentRoleBindAdded.Status,
            actualTlgAgentRoleBindAdded[0].Status);
    }

    [Fact]
    public async Task GetResponseAsync_CreatesRoleBindingsForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var roleForAuth = SanitaryInspector_DanielDe_X2024;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<TlgInput> inputHistory = 
        [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                TlgStart.CommandCode,
                tlgAgent.UserId,
                tlgAgent.ChatId,
                resultantWorkflowState: new ResultantWorkflowState(
                    glossary.GetId(typeof(UserAuthWorkflow)),
                    glossary.GetId(typeof(IUserAuthWorkflowTokenEntry))))
        ];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistory,
            roles: [SanitaryInspector_DanielDe_X2024]);
        var mockTlgAgentRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        var expectedTlgAgentRoleBindingsAdded = allModes.Select(im => 
                new TlgAgentRoleBind(
                    roleForAuth,
                    tlgAgent with { Mode = im },
                    DateTimeOffset.UtcNow,
                    Option<DateTimeOffset>.None()))
            .ToList();

        List<TlgAgentRoleBind> actualTlgAgentRoleBindingsAdded = [];
        mockTlgAgentRoleBindingsRepo
            .Setup(static x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoleBindingsAdded = tlgAgentRoles.ToList());

        await workflow.GetResponseAsync(inputValidToken);
        
        mockTlgAgentRoleBindingsRepo.Verify(static x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].TlgAgent,
                actualTlgAgentRoleBindingsAdded[i].TlgAgent);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Role,
                actualTlgAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Status,
                actualTlgAgentRoleBindingsAdded[i].Status);
        }
    }
    
    [Fact]
    public async Task GetResponseAsync_CreatesRoleBindingsForMissingModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var roleForAuth = SanitaryEngineer_DanielEn_X2024;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<TlgInput> inputHistory = 
        [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                TlgStart.CommandCode,
                tlgAgent.UserId,
                tlgAgent.ChatId,
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
                    SanitaryEngineer_DanielEn_X2024, tlgAgent with { Mode = Communications })
            ]);
        var mockTlgAgentRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];

        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        List<TlgAgentRoleBind> expectedTlgAgentRoleBindingsAdded =
        [
            // Adds missing bind for Operations Mode
            new(roleForAuth,
                tlgAgent,
                DateTimeOffset.UtcNow,
                Option<DateTimeOffset>.None()),

            // Adds missing bind for Notifications Mode
            new(roleForAuth,
                tlgAgent with { Mode = Notifications },
                DateTimeOffset.UtcNow,
                Option<DateTimeOffset>.None()),
        ];

        List<TlgAgentRoleBind> actualTlgAgentRoleBindingsAdded = [];
        mockTlgAgentRoleBindingsRepo
            .Setup(static x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoleBindingsAdded = tlgAgentRoles.ToList());
        
        var workflow = services.GetRequiredService<UserAuthWorkflow>();

        await workflow.GetResponseAsync(inputValidToken);
        
        mockTlgAgentRoleBindingsRepo.Verify(static x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].TlgAgent,
                actualTlgAgentRoleBindingsAdded[i].TlgAgent);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Role, 
                actualTlgAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Status,
                actualTlgAgentRoleBindingsAdded[i].Status);
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
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        List<TlgInput> inputHistory = 
        [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                TlgStart.CommandCode,
                tlgAgent.UserId,
                tlgAgent.ChatId,
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
                    SanitaryEngineer_DanielEn_X2024, tlgAgent with { Mode = Communications })
            ]);
        var workflow = services.GetRequiredService<UserAuthWorkflow>();
        
        var badTokenInput = inputGenerator.GetValidTlgInputTextMessage(text: badToken);
        
        var actualResponses = await workflow.GetResponseAsync(badTokenInput);
        
        Assert.Equal(
            "Bad token format! Try again...",
            actualResponses.GetValueOrThrow().Output.GetFirstRawEnglish());
    }
}