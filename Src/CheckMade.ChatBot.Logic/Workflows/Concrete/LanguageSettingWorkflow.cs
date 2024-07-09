using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

using static LanguageSettingWorkflow.States;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    LanguageSettingWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInputHistory,
        TlgInput? currentInput);
}

internal class LanguageSettingWorkflow(
        IUsersRepository usersRepo,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogicUtils logicUtils) 
    : ILanguageSettingWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        var currentState = DetermineCurrentState(inputHistory, inputHistory.LastOrDefault());
        
        return (currentState & ReceivedLanguageSetting) != 0 || 
               (currentState & Completed) != 0;
    }

    public async 
        Task<Result<(IReadOnlyCollection<OutputDto> Output, Option<Enum> NewState)>> 
        GetResponseAsync(TlgInput currentInput)
    {
        var workflowInputHistory = 
            await logicUtils.GetInteractiveSinceLastBotCommand(currentInput);
        
        return DetermineCurrentState(workflowInputHistory, currentInput) switch
        {
            Initial => 
                (new List<OutputDto> { new() 
                    { 
                        Text = Ui("🌎 Please select your preferred language:"), 
                        DomainTermSelection = new List<DomainTerm>(
                        Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>()
                            .Select(lc => Dt(lc))) 
                    } 
                }, Initial),
            
            ReceivedLanguageSetting => 
                (await SetNewLanguageAsync(currentInput), 
                    ReceivedLanguageSetting),
            
            Completed => 
                (new List<OutputDto>{ new()
                    {
                        Text = ILogicUtils.WorkflowWasCompleted
                    }},
                    Completed),
            
            _ => Result<(IReadOnlyCollection<OutputDto>, Option<Enum>)>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LanguageSettingWorkflow)}"))
        };
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInputHistory,
        TlgInput? currentInput)
    {
        if (currentInput is null)
            return Initial;
        
        var previousInputCompletedThisWorkflow = 
            workflowInputHistory.Count > 1 && 
            AnyPreviousInputContainsCallbackQuery(workflowInputHistory);
        
        return currentInput.InputType switch
        {
            TlgInputType.CallbackQuery => ReceivedLanguageSetting,
            
            _ => previousInputCompletedThisWorkflow switch
            {
                true => Completed,
                _ => Initial
            }
        };
    }

    private static bool AnyPreviousInputContainsCallbackQuery(
        IReadOnlyCollection<TlgInput> preCurrentInputHistory) =>
        preCurrentInputHistory.Any(x => 
            x.InputType.Equals(TlgInputType.CallbackQuery));

    private async Task<List<OutputDto>> SetNewLanguageAsync(TlgInput newLanguageChoice)
    {
        var domainGlossary = new DomainGlossary();
        var newLanguage = newLanguageChoice.Details.DomainTerm.GetValueOrThrow();

        if (newLanguage.EnumType != typeof(LanguageCode))
            throw new ArgumentException($"Expected a {nameof(DomainTerm)} of type {nameof(LanguageCode)}" +
                                        $"but got {nameof(newLanguage.EnumType)} instead!");

        var currentUser = (await roleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent.Equals(newLanguageChoice.TlgAgent))
            .Role.ByUser;

        await usersRepo.UpdateLanguageSettingAsync(currentUser, (LanguageCode)newLanguage.EnumValue!);
        
        return [new OutputDto 
        {
            Text = UiConcatenate(
                Ui("New language: "), 
                domainGlossary.IdAndUiByTerm[newLanguage].uiString)
        }];
    }
    
    [Flags]
    internal enum States
    {
        Initial = 1,
        ReceivedLanguageSetting = 1<<1,
        Completed = 1<<2
    }
}