using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    Task<LanguageSettingWorkflow.States> DetermineCurrentStateAsync(TlgClientPort clientPort);
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
            
            _ => Result<IReadOnlyList<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LanguageSettingWorkflow)}"))
        };
    }

    public async Task<States> DetermineCurrentStateAsync(TlgClientPort clientPort)
    {
        var allCurrentInputs = await workflowUtils.GetAllCurrentInputsAsync(clientPort);
        var lastInput = allCurrentInputs[^1];

        return lastInput.InputType switch
        {
            TlgInputType.CommandMessage => States.Initial,
            TlgInputType.CallbackQuery => States.ReceivedLanguageSetting,
            _ => States.Initial
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
        ReceivedLanguageSetting = 1<<1
    }
}