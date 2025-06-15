using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Submissions.SubmissionTypes;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.B_Details;

internal interface INewSubmissionFacilitySelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewSubmissionFacilitySelection<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IGeneralWorkflowUtils WorkflowUtils,
    ILiveEventsRepository LiveEventsRepo) 
    : INewSubmissionFacilitySelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var currentSphere = 
            GetLastSelectedSphere<T>(
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput), 
                await GetAllTradeSpecificSpheresAsync(
                    new T(),
                    currentInput.LiveEventContext.GetValueOrThrow(),
                    LiveEventsRepo));
        
        List<OutputDto> outputs =
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

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
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
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                            UiNoTranslate(" "),
                            Glossary.GetUi(selectedFacility)),
                        UpdateExistingOutputMessageId = currentInput.TlgMessageId
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