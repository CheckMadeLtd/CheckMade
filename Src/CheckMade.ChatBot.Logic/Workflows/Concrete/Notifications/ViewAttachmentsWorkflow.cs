using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal sealed record ViewAttachmentsWorkflow(
    IGeneralWorkflowUtils GeneralWorkflowUtils,   
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    ITlgInputsRepository InputsRepo,
    IServiceProvider Services,
    IDomainGlossary Glossary) 
    : WorkflowBase(GeneralWorkflowUtils, Mediator)
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        var workflowBridge =
            await BridgesRepo.GetAsync(
                currentInput.TlgAgent.ChatId, currentInput.TlgMessageId);

        if (workflowBridge is null)
            throw new InvalidOperationException(
                $"It shouldn't be possible to enter the {nameof(ViewAttachmentsWorkflow)} " +
                $"without a {nameof(workflowBridge)}");

        var issueGuid = workflowBridge.SourceInput.EntityGuid.GetValueOrThrow();
        var issueHistory = await InputsRepo.GetEntityHistoryAsync(issueGuid);
        
        var sourceTrade = (ITrade)Activator.CreateInstance(
            Glossary.GetDtType(
                workflowBridge
                    .SourceInput.ResultantWorkflow.GetValueOrThrow()
                    .InStateId))!;
        
        var issue = sourceTrade switch
        {
            SaniCleanTrade => 
                (ITradeIssueWithEvidence<SaniCleanTrade>)
                await Services.GetRequiredService<IIssueFactory<SaniCleanTrade>>()
                    .CreateAsync(issueHistory),
            
            SiteCleanTrade => 
                (ITradeIssueWithEvidence<SiteCleanTrade>)
                await Services.GetRequiredService<IIssueFactory<SiteCleanTrade>>()
                    .CreateAsync(issueHistory),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(sourceTrade)} while attempting to resolve {nameof(IIssueFactory<ITrade>)}")
        };
        
        
    }
}