// ToDo: implement just a couple of 'larger' unit tests here, testing integration of InputProcessor
// with WorkflowIdentifier and actual Workflows.
// and also cases the InputProcessor needs to handle, like inputs that interrupt the current workflow

using CheckMade.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.Tests.TestData;
using static CheckMade.Tests.ITestUtils;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic;

public class InputProcessorTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task ProcessInputAsync_ReturnsWarning_ForCallbackQuery_ToOutOfScopeInlineKeyboardButtonClick()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var serviceCollection = new UnitTestStartup().Services;
        
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        // Back to messageId: 4 because user clicked on button in chat history, but /settings now out-of-scope
        var outOfScopeCallbackQuery =
            utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.en), messageId: 4);
        
        mockTlgInputsRepo
            .Setup(x => x.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.Settings, messageId: 2),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de), messageId: 4),
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.NewIssue, messageId: 6),
                outOfScopeCallbackQuery
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        const string expectedWarningOutput = 
            "The previous workflow was completed, so your last message will be ignored.";
        var inputProcessor = _services.GetRequiredService<IInputProcessorFactory>().GetInputProcessor(tlgAgent.Mode);

        var actualOutput = await inputProcessor.ProcessInputAsync(outOfScopeCallbackQuery);
        
        Assert.Equal(expectedWarningOutput, GetFirstRawEnglish(actualOutput));
    }
    
    // [Fact]
    // public async Task ProcessInputAsync
}