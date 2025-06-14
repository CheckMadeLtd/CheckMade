using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using static CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.A_Init;

public interface INewSubmissionSphereSelection<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionSphereSelection<T> : INewSubmissionSphereSelection<T> where T : ITrade, new()
{
    private readonly ILiveEventsRepository _liveEventsRepo;
    private readonly ITrade _trade;
    private readonly IAgentRoleBindingsRepository _roleBindingsRepo;
    
    public NewSubmissionSphereSelection(
        ILiveEventsRepository liveEventsRepo,
        IDomainGlossary glossary,
        IStateMediator mediator, 
        IAgentRoleBindingsRepository roleBindingsRepo)
    {
        _liveEventsRepo = liveEventsRepo;
        Glossary = glossary;
        Mediator = mediator;
        _roleBindingsRepo = roleBindingsRepo;

        _trade = new T();
    }
    
    public IDomainGlossary Glossary { get; }
    public IStateMediator Mediator { get; }

    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        Input currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "), _trade.GetSphereOfActionLabel, UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    (await AssignedSpheresOrAllAsync(
                        currentInput, _roleBindingsRepo, _liveEventsRepo, _trade))
                    .Select(static soa => soa.Name).ToArray()),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        var relevantSphereNames = (await AssignedSpheresOrAllAsync(
                currentInput, _roleBindingsRepo, _liveEventsRepo, _trade))
            .Select(static soa => soa.Name).ToArray(); 
        
        if (currentInput.InputType is not InputType.TextMessage ||
            !relevantSphereNames
                .Contains(currentInput.Details.Text.GetValueOrThrow()))
        {
            return WorkflowResponse.CreateWarningChooseReplyKeyboardOptions(
                this, 
                relevantSphereNames);
        }

        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput, 
            Mediator.Next(typeof(INewSubmissionTypeSelection<T>)));
    }
}