using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;
using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.DomainModel.Core;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.DomainModel.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public sealed class LanguageSettingWorkflowTests
{
    private ServiceProvider? _services;
    
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
            inputs:
            [
                inputGenerator.GetValidTlgInputTextMessage(
                    text: "random decoy irrelevant to workflow")
            ]);
        var workflow = services.GetRequiredService<LanguageSettingWorkflow>();

        var actualResponse = 
            await workflow.GetResponseAsync(inputSettingsCommand);
        
        Assert.Equal(
            "🌎 Please select your preferred language:", 
            actualResponse.GetValueOrThrow().Output.GetFirstRawEnglish());
    }
    
    [Theory]
    [InlineData(LanguageCode.en)]
    [InlineData(LanguageCode.de)]
    public async Task GetResponseAsync_ReturnsSuccessMessage_AndSavesNewLanguageSetting_WhenLanguageChosen(
        LanguageCode languageCode)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        var inputSettingsCommand = inputGenerator.GetValidTlgInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings,
            resultantWorkflowState: new ResultantWorkflowState(
                glossary.GetId(typeof(LanguageSettingWorkflow)),
                glossary.GetId(typeof(ILanguageSettingSelect)))); 
        var languageSettingInput = inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
            Dt(languageCode),
            resultantWorkflowState: new ResultantWorkflowState("DDI3H3", "DL32QX"));

        var roleBind = TestRepositoryUtils.GetNewRoleBind(
            SanitaryEngineer_DanielEn_X2024,
            PrivateBotChat_Operations);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidTlgInputTextMessage(
                    text: "random decoy irrelevant to workflow"),
                inputSettingsCommand
            ],
            roles: [roleBind.Role],
            roleBindings: [roleBind]);
        var mockUserRepo = (Mock<IUsersRepository>)container.Mocks[typeof(IUsersRepository)];
        var workflow = services.GetRequiredService<LanguageSettingWorkflow>();

        var actualResponse = await workflow.GetResponseAsync(languageSettingInput);
        
        Assert.Contains(
            "New language: ",
            actualResponse.GetValueOrThrow().Output.GetAllRawEnglish());
        
        mockUserRepo.Verify(x => x.UpdateLanguageSettingAsync(
            roleBind.Role.ByUser,
            languageCode));
    }
}