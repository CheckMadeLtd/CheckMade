using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Core;
using CheckMade.Bot.Workflows.Global.LanguageSetting;
using CheckMade.Bot.Workflows.Global.LanguageSetting.States;
using General.Utils.UiTranslation;
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
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var inputSettingsCommand = inputGenerator.GetValidInputCommandMessage(
            Operations,
            (int)OperationsBotCommands.Settings); 
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidInputTextMessage(
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
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var glossary = _services.GetRequiredService<IDomainGlossary>();
        
        var inputSettingsCommand = inputGenerator.GetValidInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings,
            resultantWorkflowState: new ResultantWorkflowState(
                glossary.GetId(typeof(LanguageSettingWorkflow)),
                glossary.GetId(typeof(ILanguageSettingSelect)))); 
        var languageSettingInput = inputGenerator.GetValidInputCallbackQueryForDomainTerm(
            Dt(languageCode),
            resultantWorkflowState: new ResultantWorkflowState("DDI3H3", "DL32QX"));

        var roleBind = TestRepositoryUtils.GetNewRoleBind(
            SanitaryEngineer_DanielEn_X2024,
            PrivateBotChat_Operations);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            inputs:
            [
                inputGenerator.GetValidInputTextMessage(
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