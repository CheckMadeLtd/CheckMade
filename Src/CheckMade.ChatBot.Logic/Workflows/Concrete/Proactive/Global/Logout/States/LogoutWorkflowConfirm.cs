using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout.States;

internal interface ILogoutWorkflowConfirm : IWorkflowStateNormal;

internal sealed record LogoutWorkflowConfirm(
    IDomainGlossary Glossary, 
    IStateMediator Mediator,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo) 
    : ILogoutWorkflowConfirm
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var currentRoleBind = (await RoleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent.Equals(currentInput.TlgAgent));
        
        List<OutputDto> outputs =
        [
            new OutputDto
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

        return previousPromptFinalizer.Match<IReadOnlyCollection<OutputDto>>(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType != TlgInputType.CallbackQuery)
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
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            Ui("Logout aborted."), UiNewLines(1),
                            IInputProcessor.SeeValidBotCommandsInstruction)
                    },
                    newState: Mediator.GetTerminator(typeof(ILogoutWorkflowAborted)),
                    promptTransition: new PromptTransition(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                originalPrompt, UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.No)]),
                            UpdateExistingOutputMessageId = currentInput.TlgMessageId
                        })),
            
            _ => throw new ArgumentOutOfRangeException(nameof(selectedControl))
        };
        
        async Task<WorkflowResponse> PerformLogoutAsync()
        {
            var currentRoleBind = (await RoleBindingsRepo.GetAllActiveAsync())
                .First(tarb => tarb.TlgAgent.Equals(currentInput.TlgAgent));
        
            var roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat = 
                (await RoleBindingsRepo.GetAllActiveAsync())
                .Where(tarb =>
                    tarb.TlgAgent.UserId.Equals(currentRoleBind.TlgAgent.UserId) &&
                    tarb.TlgAgent.ChatId.Equals(currentRoleBind.TlgAgent.ChatId) &&
                    tarb.Role.Token.Equals(currentRoleBind.Role.Token))
                .ToArray();
        
            await RoleBindingsRepo
                .UpdateStatusAsync(
                    roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat, 
                    DbRecordStatus.Historic);

            return
                WorkflowResponse.Create(
                    currentInput,
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            Ui("💨 Logged out."),
                            UiNewLines(2),
                            WorkflowBase.BeginWithStart)
                    },
                    newState: Mediator.GetTerminator(typeof(ILogoutWorkflowLoggedOut)),
                    promptTransition: new PromptTransition(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                originalPrompt,
                                UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.Yes)]),
                            UpdateExistingOutputMessageId = currentInput.TlgMessageId
                        }));
        }
    }
}