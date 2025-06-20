using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Core.GIS;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Global.LanguageSetting;
using CheckMade.Bot.Workflows.Global.LanguageSetting.States;
using CheckMade.Bot.Workflows.Global.UserAuth.States;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Bot.Workflows;

public sealed class InputProcessorTests
{
    private ServiceProvider? _services;
 
    [Fact]
    public async Task ProcessInputAsync_WelcomesAndPromptsAuth_ForStartCommandOfUnauthenticatedUser()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var startCommand = inputGenerator.GetValidInputCommandMessage(
            UserId02_ChatId03_Operations.Mode,
            Start.CommandCode,
            UserId02_ChatId03_Operations.UserId,
            UserId02_ChatId03_Operations.ChatId,
            roleSetting: TestOriginatorRoleSetting.None);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories();
        var inputProcessor = services.GetRequiredService<IInputProcessor>();
    
        List<Output> expectedOutputs = 
        [
            new() { Text = Ui("🫡 Welcome to the CheckMade Bot. I shall follow your command!") },
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
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;

        var interruptingBotCommandInput =
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                (int)OperationsBotCommands.NewSubmission); 

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                // Decoys
                inputGenerator.GetValidInputCommandMessage(
                    agent.Mode,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidInputCommandMessage(
                    agent.Mode,
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
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;

        var notInterruptingBotCommandInput =
            inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                (int)OperationsBotCommands.NewSubmission);

        var glossary = _services.GetRequiredService<IDomainGlossary>();
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidInputCommandMessage(
                    agent.Mode,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidInputCallbackQueryForDomainTerm(
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
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var locationUpdate =
            inputGenerator.GetValidInputLocationMessage(
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