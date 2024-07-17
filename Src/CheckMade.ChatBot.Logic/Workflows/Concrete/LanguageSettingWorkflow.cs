using CheckMade.Common.Interfaces.ChatBot.Logic;
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
        IReadOnlyCollection<TlgInput> workflowInputHistory);
}

internal record LanguageSettingWorkflow(
        IUsersRepository UsersRepo,
        ITlgAgentRoleBindingsRepository RoleBindingsRepo,
        ILogicUtils LogicUtils,
        IDomainGlossary Glossary) 
    : ILanguageSettingWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        var currentState = DetermineCurrentState(inputHistory);
        
        return (currentState & ReceivedLanguageSetting) != 0 || 
               (currentState & Completed) != 0;
    }

    public async 
        Task<Result<WorkflowResponse>> 
        GetResponseAsync(TlgInput currentInput)
    {
        var workflowInputHistory = 
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        
        return DetermineCurrentState(workflowInputHistory) switch
        {
            Initial => 
                new WorkflowResponse(
                    new OutputDto() 
                    { 
                        Text = Ui("🌎 Please select your preferred language:"), 
                        DomainTermSelection = new List<DomainTerm>(
                        Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>()
                            .Select(lc => Dt(lc))) 
                    },
                    Glossary.GetId(Initial)),
            
            ReceivedLanguageSetting => 
                new WorkflowResponse(
                    await SetNewLanguageAsync(currentInput), 
                    Glossary.GetId(ReceivedLanguageSetting)),
            
            Completed => 
                new WorkflowResponse(
                    new OutputDto  { Text = ILogicUtils.WorkflowWasCompleted },
                    Glossary.GetId(Completed)),
            
            _ => Result<WorkflowResponse>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LanguageSettingWorkflow)}"))
        };
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInputHistory)
    {
        var lastInput = workflowInputHistory.Last();
        
        var previousInputCompletedThisWorkflow = 
            workflowInputHistory.Count > 1 && 
            AnyPreviousInputContainsCallbackQuery(workflowInputHistory.ToArray()[..^1]);
        
        return lastInput.InputType switch
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

        var currentUser = (await RoleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent.Equals(newLanguageChoice.TlgAgent))
            .Role.ByUser;

        await UsersRepo.UpdateLanguageSettingAsync(currentUser, (LanguageCode)newLanguage.EnumValue!);
        
        return [new OutputDto 
        {
            Text = UiConcatenate(
                Ui("New language: "), 
                domainGlossary.GetUi(newLanguage))
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