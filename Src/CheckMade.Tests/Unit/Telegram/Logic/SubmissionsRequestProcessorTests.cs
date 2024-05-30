using CheckMade.Common.Model.Enums;
using CheckMade.Telegram.Logic.RequestProcessors.ByBotType;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class SubmissionsRequestProcessorTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task ProcessRequestAsync_ReturnsRelevantOutput_ForProblemBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var problemCommandMessage = basics.utils.GetValidModelInputCommandMessage(
            BotType.Submissions, (int)SubmissionsBotCommands.NewIssue);

        var actualOutput = await basics.processor.ProcessRequestAsync(problemCommandMessage);
        
        Assert.True(actualOutput.IsSuccess);
        Assert.Contains(DomainCategory.SanitaryOpsIssueCleanliness,
            actualOutput.GetValueOrDefault().DomainCategorySelection.GetValueOrDefault());
    }

    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
    public async Task ProcessRequestAsync_ReturnsEchoWithAttachmentType_ForPhotoAttachmentMessage(
        AttachmentType type)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var attachmentMessage = basics.utils.GetValidModelInputTextMessageWithAttachment(type);
        
        var actualOutput = await basics.processor.ProcessRequestAsync(attachmentMessage);
        
        Assert.True(actualOutput.IsSuccess);
        Assert.Equivalent(Ui("Echo from bot {0}: {1}", BotType.Submissions, type),
            actualOutput.GetValueOrDefault().Text.GetValueOrDefault());
    }
    
    [Fact]
    public async Task ProcessRequestAsync_ReturnsNormalEcho_ForNormalResponseMessage()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var responseMessage = basics.utils.GetValidModelInputTextMessage();

        var actualOutput = await basics.processor.ProcessRequestAsync(responseMessage);
        
        Assert.True(actualOutput.IsSuccess);
        Assert.Equivalent(Ui("Echo from bot {0}: {1}", 
                BotType.Submissions, responseMessage.Details.Text.GetValueOrDefault()),
            actualOutput.GetValueOrDefault().Text.GetValueOrDefault());
    }
    
    private (ITestUtils utils, ISubmissionsRequestProcessor processor) GetBasicTestingServices(IServiceProvider sp) =>
        (sp.GetRequiredService<ITestUtils>(), sp.GetRequiredService<ISubmissionsRequestProcessor>());
}