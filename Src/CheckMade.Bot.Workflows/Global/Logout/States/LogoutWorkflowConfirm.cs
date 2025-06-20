using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Bot;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows.Global.Logout.States;

public interface ILogoutWorkflowConfirm : IWorkflowStateNormal;

public sealed record LogoutWorkflowConfirm(
    IDomainGlossary Glossary, 
    IStateMediator Mediator,
    IAgentRoleBindingsRepository RoleBindingsRepo) 
    : ILogoutWorkflowConfirm
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        var currentRoleBind = (await RoleBindingsRepo.GetAllActiveAsync())
            .First(arb => arb.Agent.Equals(currentInput.Agent));
        
        List<Output> outputs =
        [
            new Output
            {
                Text = UiConcatenate(
                    Ui("{0}, your current role is: ", 
                        currentRoleBind.Role.ByUser.FirstName),
                    Glossary.GetUi(currentRoleBind.Role.RoleType.GetType()),
                    UiNoTranslate("."), UiNewLines(1),
                    Ui("Are you sure you want to log out from this chat for {0}?", 
                        currentRoleBind.Role.AtLiveEvent.Name)),
                        
                ControlPromptsSelection = ControlPrompts.YesNo,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];

        return previousPromptFinalizer.Match<IReadOnlyCollection<Output>>(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType != InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);
        
        var controlPromptsGlossary = new ControlPromptsGlossary();
        var originalPrompt = UiIndirect(currentInput.Details.Text.GetValueOrThrow());
        var selectedControl = currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();

        return selectedControl switch
        {
            (long)ControlPrompts.Yes =>
                await PerformLogoutAsync(),

            (long)ControlPrompts.No =>
                WorkflowResponse.Create(
                    currentInput,
                    new Output
                    {
                        Text = UiConcatenate(
                            Ui("Logout aborted."), UiNewLines(1),
                            IInputProcessor.SeeValidBotCommandsInstruction)
                    },
                    newState: Mediator.GetTerminator(typeof(ILogoutWorkflowAborted)),
                    promptTransition: new PromptTransition(
                        new Output
                        {
                            Text = UiConcatenate(
                                originalPrompt, UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.No)]),
                            UpdateExistingOutputMessageId = currentInput.MessageId
                        })),
            
            _ => throw new ArgumentOutOfRangeException(nameof(selectedControl))
        };
        
        async Task<WorkflowResponse> PerformLogoutAsync()
        {
            var currentRoleBind = (await RoleBindingsRepo.GetAllActiveAsync())
                .First(arb => arb.Agent.Equals(currentInput.Agent));
        
            var roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat = 
                (await RoleBindingsRepo.GetAllActiveAsync())
                .Where(arb =>
                    arb.Agent.UserId.Equals(currentRoleBind.Agent.UserId) &&
                    arb.Agent.ChatId.Equals(currentRoleBind.Agent.ChatId) &&
                    arb.Role.Token.Equals(currentRoleBind.Role.Token))
                .ToArray();
        
            await RoleBindingsRepo
                .UpdateStatusAsync(
                    roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat, 
                    DbRecordStatus.Historic);

            return
                WorkflowResponse.Create(
                    currentInput,
                    new Output
                    {
                        Text = UiConcatenate(
                            Ui("💨 Logged out."),
                            UiNewLines(2),
                            WorkflowBase.BeginWithStart)
                    },
                    newState: Mediator.GetTerminator(typeof(ILogoutWorkflowLoggedOut)),
                    promptTransition: new PromptTransition(
                        new Output
                        {
                            Text = UiConcatenate(
                                originalPrompt,
                                UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.Yes)]),
                            UpdateExistingOutputMessageId = currentInput.MessageId
                        }));
        }
    }
}