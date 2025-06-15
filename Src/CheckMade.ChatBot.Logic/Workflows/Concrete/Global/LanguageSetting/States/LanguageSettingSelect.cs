using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;

internal interface ILanguageSettingSelect : IWorkflowStateNormal;

internal sealed record LanguageSettingSelect(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IAgentRoleBindingsRepository RoleBindingsRepo,
    IUsersRepository UsersRepo,
    ILastOutputMessageIdCache MsgIdCache) 
    : ILanguageSettingSelect
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new OutputDto
            {
                Text = Ui("🌎 Please select your preferred language:"),
                DomainTermSelection = new List<DomainTerm>(
                    Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>()
                        .Select(static lc => Dt(lc))),
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
        if (currentInput.InputType != InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);
        
        var newLanguage = currentInput.Details.DomainTerm.GetValueOrThrow();
        
        var currentUser = (await RoleBindingsRepo.GetAllActiveAsync())
            .First(arb => arb.TlgAgent.Equals(currentInput.TlgAgent))
            .Role.ByUser;

        await UsersRepo.UpdateLanguageSettingAsync(currentUser, (LanguageCode)newLanguage.EnumValue!);

        return WorkflowResponse.Create(
            currentInput,
            new OutputDto
            {
                Text = UiConcatenate(
                    Ui("New language: "),
                    Glossary.GetUi(newLanguage))
            },
            newState: Mediator.GetTerminator(typeof(ILanguageSettingSet)),
            promptTransition: new PromptTransition(currentInput.MessageId, MsgIdCache, currentInput.TlgAgent)
        );
    }
}