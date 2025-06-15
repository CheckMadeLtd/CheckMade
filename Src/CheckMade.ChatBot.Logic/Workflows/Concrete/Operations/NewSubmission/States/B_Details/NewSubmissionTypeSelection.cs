using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Common.Domain.Data.Core.Submissions.SubmissionTypes;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.B_Details;

internal interface INewSubmissionTypeSelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewSubmissionTypeSelection<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator) 
    : INewSubmissionTypeSelection<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var currentRoleType = currentInput.OriginatorRole.GetValueOrThrow().RoleType;
        var skipCleaningRelatedSubmissions = currentRoleType is TradeTeamLead<T>; 
        
        List<OutputDto> outputs =
        [
            new()
            {
                Text = Ui("Please select the type of submission:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary
                        .GetAll(typeof(ITradeSubmission<T>))
                        .SkipWhile(dt => skipCleaningRelatedSubmissions && 
                                         (dt.TypeValue == typeof(CleaningIssue<T>) ||
                                          dt.TypeValue == typeof(Assessment<T>)))
                        .ToImmutableArray()),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId,
                ControlPromptsSelection = ControlPrompts.Back
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
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
                    new OutputDto
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