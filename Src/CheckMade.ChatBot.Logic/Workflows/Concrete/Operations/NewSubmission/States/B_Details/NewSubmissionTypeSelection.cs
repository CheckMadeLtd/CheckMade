using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.DomainModel.ChatBot;
using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.ChatBot.Output;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Common.DomainModel.Core.Submissions.SubmissionTypes;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.DomainModel.Interfaces.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.B_Details;

internal interface INewSubmissionTypeSelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewSubmissionTypeSelection<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator) 
    : INewSubmissionTypeSelection<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
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

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
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
                        UpdateExistingOutputMessageId = currentInput.TlgMessageId
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