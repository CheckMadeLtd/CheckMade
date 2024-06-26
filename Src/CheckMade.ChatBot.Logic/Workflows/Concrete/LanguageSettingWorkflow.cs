using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    LanguageSettingWorkflow.States DetermineCurrentState(IReadOnlyCollection<TlgInput> history);
}

internal class LanguageSettingWorkflow(
        IUsersRepository usersRepo,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogicUtils logicUtils) 
    : ILanguageSettingWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> history)
    {
        var currentState = DetermineCurrentState(history);
        
        return (currentState & States.ReceivedLanguageSetting) != 0 || 
               (currentState & States.Completed) != 0;
    }

    public async Task<Result<IReadOnlyCollection<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        var recentHistory = await logicUtils.GetInputsForCurrentWorkflow(tlgInput.TlgAgent);
        
        return DetermineCurrentState(recentHistory) switch
        {
            States.Initial => new List<OutputDto>
            {
                new()
                {
                    Text = Ui("🌎 Please select your preferred language:"),
                    DomainTermSelection = new List<DomainTerm>(
                        Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>()
                            .Select(lc => Dt(lc)))
                }
            },
            
            States.ReceivedLanguageSetting => await SetNewLanguageAsync(tlgInput),
            
            States.Completed => new List<OutputDto>{ new() { Text = ILogicUtils.WorkflowWasCompleted }},
            
            _ => Result<IReadOnlyCollection<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LanguageSettingWorkflow)}"))
        };
    }

    public States DetermineCurrentState(IReadOnlyCollection<TlgInput> history)
    {
        var lastInput = history.Last();

        var previousInputCompletedThisWorkflow = 
            history.Count > 1 && 
            AnyPreviousInputContainsCallbackQuery(history.ToArray()[..^1]);
        
        return lastInput.InputType switch
        {
            TlgInputType.CallbackQuery => States.ReceivedLanguageSetting,
            
            _ => previousInputCompletedThisWorkflow switch
            {
                true => States.Completed,
                _ => States.Initial
            }
        };
    }

    private static bool AnyPreviousInputContainsCallbackQuery(IReadOnlyCollection<TlgInput> recentHistory) =>
        recentHistory.Any(x => x.InputType == TlgInputType.CallbackQuery);

    private async Task<List<OutputDto>> SetNewLanguageAsync(TlgInput newLanguageInput)
    {
        var domainGlossary = new DomainGlossary();
        var newLanguage = newLanguageInput.Details.DomainTerm.GetValueOrThrow();

        if (newLanguage.EnumType != typeof(LanguageCode))
            throw new ArgumentException($"Expected a {nameof(DomainTerm)} of type {nameof(LanguageCode)}" +
                                        $"but got {nameof(newLanguage.EnumType)} instead!");

        var currentUser = (await roleBindingsRepo.GetAllAsync())
            .First(arb => 
                arb.TlgAgent == newLanguageInput.TlgAgent &&
                arb.Status == DbRecordStatus.Active)
            .Role.User;

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