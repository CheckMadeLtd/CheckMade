using CheckMade.Common.Interfaces.Persistence.ChatBot;
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
        ITlgClientPortRoleRepository portRoleRepo,    
        IWorkflowUtils workflowUtils) 
    : ILanguageSettingWorkflow
{
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return await DetermineCurrentStateAsync(tlgInput.ClientPort) switch
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

        var secondToLastInput = allCurrentInputs.Count > 1
            ? allCurrentInputs[^2]
            : Option<TlgInput>.None();

        return lastInput.InputType switch
        {
            TlgInputType.CommandMessage => States.Initial,
            
            TlgInputType.CallbackQuery => States.ReceivedLanguageSetting,
            
            _ => secondToLastInput.Match(
                stl => stl.InputType switch
                {
                    TlgInputType.CallbackQuery => States.Completed,
                    _ => States.Initial
                },
                () => States.Initial)
        };
    }

    private static async Task<List<OutputDto>> SetNewLanguageAsync(TlgInput newLanguageInput)
    {
        var domainGlossary = new DomainGlossary();
        var newLanguage = newLanguageInput.Details.DomainTerm.GetValueOrThrow();
        
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