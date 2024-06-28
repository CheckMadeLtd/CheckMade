using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    LanguageSettingWorkflow.States DetermineCurrentState(IReadOnlyCollection<TlgInput> workflowInputHistory);
}

internal class LanguageSettingWorkflow(
        IUsersRepository usersRepo,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogicUtils logicUtils) 
    : ILanguageSettingWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        var currentState = DetermineCurrentState(inputHistory);
        
        return (currentState & States.ReceivedLanguageSetting) != 0 || 
               (currentState & States.Completed) != 0;
    }

    public async Task<Result<IReadOnlyCollection<OutputDto>>> GetResponseAsync(TlgInput currentInput)
    {
        var workflowInputHistory = 
            await logicUtils.GetInputsSinceLastBotCommand(currentInput.TlgAgent);
        
        return DetermineCurrentState(workflowInputHistory) switch
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
            
            States.ReceivedLanguageSetting => await SetNewLanguageAsync(currentInput),
            
            States.Completed => new List<OutputDto>{ new() { Text = ILogicUtils.WorkflowWasCompleted }},
            
            _ => Result<IReadOnlyCollection<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LanguageSettingWorkflow)}"))
        };
    }

    public States DetermineCurrentState(IReadOnlyCollection<TlgInput> workflowInputHistory)
    {
        var lastInput = workflowInputHistory.Last();

        var previousInputCompletedThisWorkflow = 
            workflowInputHistory.Count > 1 && 
            AnyPreviousInputContainsCallbackQuery(workflowInputHistory.ToArray()[..^1]);
        
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

    private static bool AnyPreviousInputContainsCallbackQuery(
        IReadOnlyCollection<TlgInput> preCurrentInputHistory) =>
        preCurrentInputHistory.Any(x => x.InputType == TlgInputType.CallbackQuery);

    private async Task<List<OutputDto>> SetNewLanguageAsync(TlgInput newLanguageChoice)
    {
        var domainGlossary = new DomainGlossary();
        var newLanguage = newLanguageChoice.Details.DomainTerm.GetValueOrThrow();

        if (newLanguage.EnumType != typeof(LanguageCode))
            throw new ArgumentException($"Expected a {nameof(DomainTerm)} of type {nameof(LanguageCode)}" +
                                        $"but got {nameof(newLanguage.EnumType)} instead!");

        var currentUser = (await roleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent == newLanguageChoice.TlgAgent)
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