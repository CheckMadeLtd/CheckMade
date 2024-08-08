using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public sealed class LanguageSettingWorkflowTests
{
    private ServiceProvider? _services;
    
    // [Theory]
    // // Establishes that 'Settings' is always the same int for all InteractionModes / Bots.
    // [InlineData((int)OperationsBotCommands.Settings)]
    // [InlineData((int)CommunicationsBotCommands.Settings)]
    // [InlineData((int)NotificationsBotCommands.Settings)]
    // [SuppressMessage("Usage", "xUnit1025:InlineData should be unique within the Theory it belongs to")]
    // public void DetermineCurrentState_ReturnsInitial_WhenLastInputWasBotCommand(int botCommand)
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     
    //     var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
    //     var tlgAgent = PrivateBotChat_Operations;
    //
    //     List<TlgInput> inputHistory = [
    //         inputGenerator.GetValidTlgInputTextMessage(),
    //         inputGenerator.GetValidTlgInputCommandMessage(
    //             tlgAgent.Mode, botCommand)];
    //     
    //     var serviceCollection = new UnitTestStartup().Services;
    //     var (services, _) = serviceCollection.ConfigureTestRepositories(
    //         inputs: inputHistory);
    //     var workflow = services.GetRequiredService<ILanguageSettingWorkflow>();
    //     
    //     var actualState = 
    //         workflow.DetermineCurrentState(inputHistory);
    //     
    //     Assert.Equal(
    //         States.Initial, 
    //         actualState);
    // }
    //
    // [Fact]
    // public void DetermineCurrentState_ReturnsReceived_WhenLastInputWasCallbackQuery()
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     
    //     var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
    //     var tlgAgent = PrivateBotChat_Operations;
    //
    //     List<TlgInput> inputHistory = [ 
    //         inputGenerator.GetValidTlgInputTextMessage(),
    //         inputGenerator.GetValidTlgInputCommandMessage(
    //             tlgAgent.Mode, (int)OperationsBotCommands.Settings),
    //         inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
    //             Dt(LanguageCode.de))];
    //     
    //     var serviceCollection = new UnitTestStartup().Services;
    //     var (services, _) = serviceCollection.ConfigureTestRepositories(
    //         inputs: inputHistory);
    //     var workflow = services.GetRequiredService<ILanguageSettingWorkflow>();
    //     
    //     var actualState = 
    //         workflow.DetermineCurrentState(inputHistory);
    //     
    //     Assert.Equal(
    //         States.ReceivedLanguageSetting,
    //         actualState);
    // }
    //
    // [Fact]
    // public void DetermineCurrentState_ReturnsCompleted_WhenLastInputIsAfterCallbackQueryInput()
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     
    //     var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
    //     var tlgAgent = PrivateBotChat_Operations;
    //
    //     List<TlgInput> inputHistory = [
    //         inputGenerator.GetValidTlgInputCommandMessage(
    //             tlgAgent.Mode, (int)OperationsBotCommands.Settings),
    //         inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
    //             Dt(LanguageCode.de)),
    //         inputGenerator.GetValidTlgInputTextMessage()]; 
    //     
    //     var serviceCollection = new UnitTestStartup().Services;
    //     var (services, _) = serviceCollection.ConfigureTestRepositories(
    //         inputs: inputHistory);
    //     var workflow = services.GetRequiredService<ILanguageSettingWorkflow>();
    //     
    //     var actualState = 
    //         workflow.DetermineCurrentState(inputHistory);
    //     
    //     Assert.Equal(
    //         States.Completed,
    //         actualState);
    // }
    
    [Fact]
    public async Task GetResponseAsync_ShowsLanguageSelectionMenu_InInitialState()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var inputSettingsCommand = inputGenerator.GetValidTlgInputCommandMessage(
            Operations,
            (int)OperationsBotCommands.Settings); 
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[]
            {
                inputGenerator.GetValidTlgInputTextMessage(
                    text: "random decoy irrelevant to workflow")
            });
        var workflow = services.GetRequiredService<LanguageSettingWorkflow>();

        var actualResponse = 
            await workflow.GetResponseAsync(inputSettingsCommand);
        
        Assert.Equal(
            "🌎 Please select your preferred language:", 
            TestUtils.GetFirstRawEnglish(actualResponse.GetValueOrThrow().Output));
    }
    
    [Theory(Skip = "Needs fixing after refactoring of workflow")]
    [InlineData(LanguageCode.en)]
    [InlineData(LanguageCode.de)]
    public async Task GetResponseAsync_ReturnsSuccessMessage_AndSavesNewLanguageSetting_WhenLanguageChosen(
        LanguageCode languageCode)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        
        var inputSettingsCommand = inputGenerator.GetValidTlgInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings); 
        var languageSettingInput = inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
            Dt(languageCode),
            resultantWorkflowState: new ResultantWorkflowState("DDI3H3", "DL32QX"));

        var roleBind = TestRepositoryUtils.GetNewRoleBind(
            SaniCleanEngineer_DanielEn_X2024,
            PrivateBotChat_Operations);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs: new[]
            {
                inputGenerator.GetValidTlgInputTextMessage(
                    text: "random decoy irrelevant to workflow"),
                inputSettingsCommand,
            },
            roles: new []{ roleBind.Role },
            roleBindings: new []{ roleBind });
        var mockUserRepo = (Mock<IUsersRepository>)container.Mocks[typeof(IUsersRepository)];
        var workflow = services.GetRequiredService<LanguageSettingWorkflow>();

        var actualResponse = await workflow.GetResponseAsync(languageSettingInput);
        
        Assert.StartsWith(
            "New language: ",
            TestUtils.GetFirstRawEnglish(actualResponse.GetValueOrThrow().Output));
        
        mockUserRepo.Verify(x => x.UpdateLanguageSettingAsync(
            roleBind.Role.ByUser,
            languageCode));
    }
}