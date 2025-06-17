using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.Submissions.SubmissionTypes;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Core;
using CheckMade.Bot.Workflows.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Workflows.Operations.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Workflows.Operations.NewSubmission.States.B_Details;

public interface INewSubmissionFacilitySelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

public sealed record NewSubmissionFacilitySelection<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IGeneralWorkflowUtils WorkflowUtils,
    ILiveEventsRepository LiveEventsRepo) 
    : INewSubmissionFacilitySelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        var currentSphere = 
            GetLastSelectedSphere<T>(
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput), 
                await GetAllTradeSpecificSpheresAsync(
                    new T(),
                    currentInput.LiveEventContext.GetValueOrThrow(),
                    LiveEventsRepo));
        
        List<Output> outputs =
        [
            new()
            {
                Text = Ui("Choose affected facility:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary
                        .GetAll(typeof(IFacility))
                        .Where(dt => currentSphere.Details.AvailableFacilities.Contains(dt))
                        .ToImmutableArray()),
                ControlPromptsSelection = ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType is not InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var selectedFacility = currentInput.Details.DomainTerm.GetValueOrThrow();
            
            var currentSubmissionTypeName =
                NewSubmissionUtils.GetLastSubmissionType(
                        await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput))
                    .Name
                    .GetTypeNameWithoutGenericParamSuffix();
            
            var promptTransition =
                new PromptTransition(
                    new Output
                    {
                        Text = UiConcatenate(
                            UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                            UiNoTranslate(" "),
                            Glossary.GetUi(selectedFacility)),
                        UpdateExistingOutputMessageId = currentInput.MessageId
                    });
            
            return currentSubmissionTypeName switch
            {
                nameof(Assessment<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput,
                        Mediator.Next(typeof(INewSubmissionAssessmentRating<T>)),
                        promptTransition),
                
                _ => await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionEvidenceEntry<T>)),
                    promptTransition) 
            };
        }

        return // on ControlPrompts.Back
            await WorkflowResponse.CreateFromNextStateAsync(
                currentInput,
                Mediator.Next(typeof(INewSubmissionTypeSelection<T>)),
                new PromptTransition(true));
    }
}