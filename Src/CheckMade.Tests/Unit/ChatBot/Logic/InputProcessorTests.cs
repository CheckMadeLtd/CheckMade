using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth.States;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public sealed class InputProcessorTests
{
    private ServiceProvider? _services;
 
    [Fact]
    public async Task ProcessInputAsync_WelcomesAndPromptsAuth_ForStartCommandOfUnauthenticatedUser()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var startCommand = inputGenerator.GetValidTlgInputCommandMessage(
            UserId02_ChatId03_Operations.Mode,
            TlgStart.CommandCode,
            UserId02_ChatId03_Operations.UserId,
            UserId02_ChatId03_Operations.ChatId,
            roleSetting: TestOriginatorRoleSetting.None);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories();
        var inputProcessor = services.GetRequiredService<IInputProcessor>();
    
        List<OutputDto> expectedOutputs = 
        [
            new() { Text = Ui("🫡 Welcome to the CheckMade ChatBot. I shall follow your command!") },
            new() { Text = UserAuthWorkflowTokenEntry.EnterTokenPrompt }
        ];
        
        var result = 
            await inputProcessor
                .ProcessInputAsync(startCommand);
    
        Assert.Equivalent(
            expectedOutputs,
            result.ResultingOutputs);
    }
    
    [Fact]
    public async Task ProcessInputAsync_PrefixesWarning_WhenUserInterruptedPreviousWorkflow_WithNewBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var interruptingBotCommandInput =
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                (int)OperationsBotCommands.NewIssue); 

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                // Decoys
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings)
            ]);
        var inputProcessor = services.GetRequiredService<IInputProcessor>();
        
        const string expectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        
        var result =
            await inputProcessor
                .ProcessInputAsync(interruptingBotCommandInput);
        
        Assert.Contains(
            expectedWarningOutput,
            result.ResultingOutputs.GetAllRawEnglish());
    }
    
    [Fact]
    public async Task ProcessInputAsync_NoWarning_ForNewBotCommand_WhenUserCompletedPreviousWorkflow()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var notInterruptingBotCommandInput =
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                (int)OperationsBotCommands.NewIssue);

        var glossary = _services.GetRequiredService<IDomainGlossary>();
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de),
                    resultantWorkflowState: new ResultantWorkflowState(
                        glossary.GetId(typeof(LanguageSettingWorkflow)),
                        glossary.GetId(typeof(ILanguageSettingSet))))
            ]);
        var inputProcessor = services.GetRequiredService<IInputProcessor>();
        
        const string notExpectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        
        var result = 
            await inputProcessor
                .ProcessInputAsync(notInterruptingBotCommandInput);
        
        Assert.DoesNotContain(
            notExpectedWarningOutput,
            result.ResultingOutputs.GetAllRawEnglish());
    }

    [Fact]
    public async Task ProcessInputAsync_ReturnsEmptyOutput_ForLocationUpdate()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var locationUpdate =
            inputGenerator.GetValidTlgInputLocationMessage(
                new Geo(17, -22, Option<double>.None()));
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories();
        var inputProcessor = services.GetRequiredService<IInputProcessor>();

        var result =
            await inputProcessor
                .ProcessInputAsync(locationUpdate);
        
        Assert.Empty(result.ResultingOutputs);
    }
}