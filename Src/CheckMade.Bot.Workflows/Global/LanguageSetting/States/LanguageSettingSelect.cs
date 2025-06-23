using System.Collections.Immutable;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Global.LanguageSetting.States;

public interface ILanguageSettingSelect : IWorkflowStateNormal;

public sealed record LanguageSettingSelect(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IAgentRoleBindingsRepository RoleBindingsRepo,
    IUsersRepository UsersRepo,
    ILastOutputMessageIdCache MsgIdCache) 
    : ILanguageSettingSelect
{
    public Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        List<Output> outputs =
        [
            new Output
            {
                Text = Ui("🌎 Please select your preferred language:"),
                DomainTermSelection = new List<DomainTerm>(
                    Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>()
                        .Select(static lc => Dt(lc))),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<Output>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType != InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);
        
        var newLanguage = currentInput.Details.DomainTerm.GetValueOrThrow();
        
        var currentUser = (await RoleBindingsRepo.GetAllActiveAsync())
            .First(arb => arb.Agent.Equals(currentInput.Agent))
            .Role.ByUser;

        await UsersRepo.UpdateLanguageSettingAsync(currentUser, (LanguageCode)newLanguage.EnumValue!);

        return WorkflowResponse.Create(
            currentInput,
            new Output
            {
                Text = UiConcatenate(
                    Ui("New language: "),
                    Glossary.GetUi(newLanguage))
            },
            newState: Mediator.GetTerminator(typeof(ILanguageSettingSet)),
            promptTransition: new PromptTransition(currentInput.MessageId, MsgIdCache, currentInput.Agent)
        );
    }
}