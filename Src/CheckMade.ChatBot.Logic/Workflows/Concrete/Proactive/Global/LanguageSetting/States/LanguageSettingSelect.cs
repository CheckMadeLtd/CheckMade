using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;

internal interface ILanguageSettingSelect : IWorkflowStateNormal;

internal sealed record LanguageSettingSelect(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo,
    IUsersRepository UsersRepo) 
    : ILanguageSettingSelect
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<TlgMessageId> inPlaceUpdateMessageId,
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
        if (currentInput.InputType != TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);
        
        var newLanguage = currentInput.Details.DomainTerm.GetValueOrThrow();
        
        var currentUser = (await RoleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent.Equals(currentInput.TlgAgent))
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
            promptTransition: new PromptTransition(currentInput.TlgMessageId)
        );
    }
}