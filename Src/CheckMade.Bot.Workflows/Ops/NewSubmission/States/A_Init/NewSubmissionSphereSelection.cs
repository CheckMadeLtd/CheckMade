using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Data.Bot;
using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Data.Bot.Output;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Core;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;

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
    
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        List<Output> outputs =
        [
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "), _trade.GetSphereOfActionLabel, UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    (await AssignedSpheresOrAllAsync(
                        currentInput, _roleBindingsRepo, _liveEventsRepo, _trade))
                    .Select(SphereLabelComposer)
                    .Order()
                    .ToArray()),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        var relevantSphereLabels = (await AssignedSpheresOrAllAsync(
                currentInput, _roleBindingsRepo, _liveEventsRepo, _trade))
            .Select(SphereLabelComposer)
            .Order()
            .ToArray(); 
        
        if (currentInput.InputType is not InputType.TextMessage ||
            !relevantSphereLabels
                .Contains(currentInput.Details.Text.GetValueOrThrow()))
        {
            return WorkflowResponse.CreateWarningChooseReplyKeyboardOptions(
                this, 
                relevantSphereLabels);
        }

        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput, 
            Mediator.Next(typeof(INewSubmissionTypeSelection<T>)));
    }
}