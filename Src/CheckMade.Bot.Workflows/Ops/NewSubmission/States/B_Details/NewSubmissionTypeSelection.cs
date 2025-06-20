using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Common.Actors.RoleTypes;
using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using CheckMade.Abstract.Domain.Model.Common.Submissions;
using CheckMade.Abstract.Domain.Model.Common.Submissions.SubmissionTypes;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;

// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;

public interface INewSubmissionTypeSelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

public sealed record NewSubmissionTypeSelection<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    ILiveEventsRepository LiveEventsRepo) 
    : INewSubmissionTypeSelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        List<Output> outputs =
        [
            new()
            {
                Text = Ui("Please select the type of submission:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary
                        .GetAll(typeof(ITradeSubmission<T>))
                        .Except(await GetExcludedSubmissionTypesAsync())
                        .ToImmutableArray()),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId,
                ControlPromptsSelection = ControlPrompts.Back
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());

        async Task<IReadOnlyCollection<DomainTerm>> GetExcludedSubmissionTypesAsync()
        {
            var skipCleaningRelatedSubmissions = 
                currentInput.OriginatorRole
                    .GetValueOrThrow()
                    .RoleType is TradeTeamLead<T>;

            var skipMissingConsumablesSubmission = 
                await currentInput
                    .Apply(WorkflowUtils.GetInteractiveWorkflowHistoryAsync)
                    .Apply(async history => 
                        await GetAvailableConsumablesAsync<T>(await history, currentInput, LiveEventsRepo))
                    .Apply(async static consumables => (await consumables).Count == 0);
            
            var exclusions = new List<DomainTerm>();
    
            if (skipCleaningRelatedSubmissions)
            {
                exclusions.Add(Dt(typeof(CleaningIssue<T>)));
                exclusions.Add(Dt(typeof(Assessment<T>)));
            }
    
            if (skipMissingConsumablesSubmission)
            {
                exclusions.Add(Dt(typeof(ConsumablesIssue<T>)));
            }
    
            return exclusions;
        }
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType is not InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var submissionTypeName = 
                currentInput.Details.DomainTerm.GetValueOrThrow()
                    .TypeValue!.Name
                    .GetTypeNameWithoutGenericParamSuffix();

            var promptTransition =
                new PromptTransition(
                    new Output
                    {
                        Text = UiConcatenate(
                            UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                            UiNoTranslate(" "),
                            Glossary.GetUi(currentInput.Details.DomainTerm.GetValueOrThrow())),
                        UpdateExistingOutputMessageId = currentInput.MessageId
                    });
            
            return submissionTypeName switch
            {
                nameof(CleaningIssue<T>) or nameof(TechnicalIssue<T>) or nameof(Assessment<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput, 
                        Mediator.Next(typeof(INewSubmissionFacilitySelection<T>)),
                        promptTransition),
            
                nameof(ConsumablesIssue<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput, 
                        Mediator.Next(typeof(INewSubmissionConsumablesSelection<T>)),
                        promptTransition),
            
                nameof(StaffIssue<T>) or nameof(GeneralIssue<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput, 
                        Mediator.Next(typeof(INewSubmissionEvidenceEntry<T>)),
                        promptTransition),
                
                _ => throw new InvalidOperationException(
                    $"Unhandled {nameof(currentInput.Details.DomainTerm)}: '{submissionTypeName}'")
            };
        }

        return // on ControlPrompts.Back 
            await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, 
                Mediator.Next(typeof(INewSubmissionSphereSelection<T>)),
                new PromptTransition(true));
    }
}