using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;

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
        Option<int> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new OutputDto
            {
                Text = Ui("🌎 Please select your preferred language:"),
                DomainTermSelection = new List<DomainTerm>(
                    Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>()
                        .Select(lc => Dt(lc))),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableReadOnlyCollection(),
                () => outputs.ToImmutableReadOnlyCollection()));
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
            newState: Mediator.Terminate(typeof(ILanguageSettingSet)),
            promptTransition: new PromptTransition(currentInput.TlgMessageId)
        );
    }
}