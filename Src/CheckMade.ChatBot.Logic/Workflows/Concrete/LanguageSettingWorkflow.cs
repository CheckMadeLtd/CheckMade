using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    Task<LanguageSettingWorkflow.States> DetermineCurrentStateAsync(TlgAgent tlgAgent);
}

internal class LanguageSettingWorkflow(
        IUserRepository userRepo,
        IWorkflowUtils workflowUtils) 
    : ILanguageSettingWorkflow
{
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return await DetermineCurrentStateAsync(tlgInput.TlgAgent) switch
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
            
            States.Completed => new List<OutputDto>{ new() { Text = IWorkflowUtils.WorkflowWasCompleted }},
            
            _ => Result<IReadOnlyList<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LanguageSettingWorkflow)}"))
        };
    }

    public async Task<States> DetermineCurrentStateAsync(TlgAgent tlgAgent)
    {
        var allCurrentInputs = await workflowUtils.GetAllCurrentInputsAsync(tlgAgent);
        var lastInput = allCurrentInputs[^1];

        var previousInputCompletedThisWorkflow = 
            allCurrentInputs.Count > 1 && 
            AnyPreviousInputContainsCallbackQuery(allCurrentInputs.ToArray()[..^2]);
        
        return lastInput.InputType switch
        {
            TlgInputType.CommandMessage => States.Initial,
            
            TlgInputType.CallbackQuery => States.ReceivedLanguageSetting,
            
            _ => previousInputCompletedThisWorkflow switch
            {
                true => States.Completed,
                _ => States.Initial
            }
        };
    }

    private static bool AnyPreviousInputContainsCallbackQuery(IReadOnlyList<TlgInput> previousInputs) =>
        previousInputs.Any(x => x.InputType == TlgInputType.CallbackQuery);

    private async Task<List<OutputDto>> SetNewLanguageAsync(TlgInput newLanguageInput)
    {
        var domainGlossary = new DomainGlossary();
        var newLanguage = newLanguageInput.Details.DomainTerm.GetValueOrThrow();

        if (newLanguage.EnumType != typeof(LanguageCode))
            throw new ArgumentException($"Expected a {nameof(DomainTerm)} of type {nameof(LanguageCode)}" +
                                        $"but got {nameof(newLanguage.EnumType)} instead!");

        var currentUser = workflowUtils.GetAllTlgAgentRoles()
            .First(arb => arb.TlgAgent == newLanguageInput.TlgAgent)
            .Role.User;

        await userRepo.UpdateLanguageSettingAsync(currentUser, (LanguageCode)newLanguage.EnumValue!);
        
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