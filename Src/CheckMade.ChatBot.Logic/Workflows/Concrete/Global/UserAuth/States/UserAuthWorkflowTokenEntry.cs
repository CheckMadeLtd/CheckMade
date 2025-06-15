using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;
using static CheckMade.Common.Utils.Validators.InputValidator;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth.States;

internal interface IUserAuthWorkflowTokenEntry : IWorkflowStateNormal;

internal sealed record UserAuthWorkflowTokenEntry(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IRolesRepository RolesRepo,
    IAgentRoleBindingsRepository RoleBindingsRepo) 
    : IUserAuthWorkflowTokenEntry
{
    internal static readonly UiString EnterTokenPrompt = 
        Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample());
    
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs = 
        [
            new OutputDto
            {
                Text = EnterTokenPrompt,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType != InputType.TextMessage)
            return WorkflowResponse.CreateWarningEnterTextOnly(this);

        var enteredText = currentInput.Details.Text.GetValueOrThrow();

        return enteredText.IsValidToken() switch
        {
            true => await TokenExistsAsync() switch
            {
                true => await AuthenticateUserAsync(currentInput),

                _ => WorkflowResponse.Create(
                    currentInput,
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            Ui("This is an unknown token. Try again..."),
                            UiNewLines(1), EnterTokenPrompt)
                    },
                    newState: this)
            },

            _ => WorkflowResponse.Create(
                currentInput,
                new OutputDto
                {
                    Text = Ui("Bad token format! Try again...")
                },
                newState: this)
        };

        async Task<bool> TokenExistsAsync() =>
            (await RolesRepo.GetAllAsync())
            .Any(role => role.Token.Equals(enteredText));

        async Task<WorkflowResponse> AuthenticateUserAsync(TlgInput tokenInputAttempt)
        {
            var inputText = tokenInputAttempt.Details.Text.GetValueOrThrow();
            var currentMode = tokenInputAttempt.TlgAgent.Mode;

            var outputs = new List<OutputDto>();

            var newAgentRoleBindForCurrentMode = new AgentRoleBind(
                (await RolesRepo.GetAllAsync()).First(r => r.Token.Equals(inputText)),
                tokenInputAttempt.TlgAgent with { Mode = currentMode },
                DateTimeOffset.UtcNow,
                Option<DateTimeOffset>.None());

            var preExistingActiveRoleBindings =
                (await RoleBindingsRepo.GetAllActiveAsync());

            var lastActiveRoleBindForCurrentMode =
                FirstOrDefaultPreExistingActiveRoleBind(currentMode);

            var roleTypeUiString =
                Glossary.GetUi(newAgentRoleBindForCurrentMode.Role.RoleType.GetType());

            if (lastActiveRoleBindForCurrentMode != null)
            {
                await RoleBindingsRepo.UpdateStatusAsync(lastActiveRoleBindForCurrentMode, DbRecordStatus.Historic);

                outputs.Add(new OutputDto
                {
                    Text = UiConcatenate(
                        Ui("""
                           Warning: you were already authenticated with this token in another {0} chat.
                           This will be the new {0} chat where you receive messages at {1}, in your role as: 
                           """,
                            currentMode,
                            newAgentRoleBindForCurrentMode.Role.AtLiveEvent.Name),
                        roleTypeUiString)
                });
            }

            outputs.Add(new OutputDto
            {
                Text = UiConcatenate(
                    Ui("{0}, you have successfully authenticated at live-event {1} in your role as: ",
                        newAgentRoleBindForCurrentMode.Role.ByUser.FirstName,
                        newAgentRoleBindForCurrentMode.Role.AtLiveEvent.Name),
                    roleTypeUiString)
            });

            outputs.Add(new OutputDto
            {
                Text = IInputProcessor.SeeValidBotCommandsInstruction
            });

            var agentRoleBindingsToAdd = new List<AgentRoleBind> { newAgentRoleBindForCurrentMode };

            var isInputInPrivateBotChat =
                tokenInputAttempt.TlgAgent.ChatId.Id.Equals(
                    tokenInputAttempt.TlgAgent.UserId.Id);

            if (isInputInPrivateBotChat)
            {
                AddAgentRoleBindingsForOtherModes();
            }

            await RoleBindingsRepo.AddAsync(agentRoleBindingsToAdd);

            return new WorkflowResponse(
                outputs,
                Option<string>.Some(
                    Glossary.GetId(typeof(IUserAuthWorkflowAuthenticated))),
                Option<Guid>.None());

            AgentRoleBind? FirstOrDefaultPreExistingActiveRoleBind(InteractionMode mode) =>
                preExistingActiveRoleBindings.FirstOrDefault(arb =>
                    arb.Role.Token.Equals(inputText) &&
                    arb.TlgAgent.Mode.Equals(mode));

            void AddAgentRoleBindingsForOtherModes()
            {
                var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
                var otherModes = allModes.Except([currentMode]);

                agentRoleBindingsToAdd.AddRange(
                    from mode in otherModes
                    where FirstOrDefaultPreExistingActiveRoleBind(mode) == null
                    select newAgentRoleBindForCurrentMode with
                    {
                        TlgAgent = newAgentRoleBindForCurrentMode.TlgAgent with { Mode = mode },
                        ActivationDate = DateTimeOffset.UtcNow
                    });
            }
        }
    }
}