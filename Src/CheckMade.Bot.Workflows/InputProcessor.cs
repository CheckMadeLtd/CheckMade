using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows;

public sealed class InputProcessor(
    IGeneralWorkflowUtils workflowUtils,
    IDomainGlossary glossary,
    ILastOutputMessageIdCache msgIdCache)
    : IInputProcessor
{
    public async Task<(Option<Input> EnrichedOriginalInput, IReadOnlyCollection<Output> ResultingOutputs)> 
        ProcessInputAsync(Result<Input> input)
    {
        return await input.Match(
            async currentInput =>
            {
                if (currentInput.InputType == InputType.Location)
                {
                    return (currentInput, []);
                }
                
                List<Output> outputBuilder = [];

                if (IsStartCommand(currentInput))
                {
                    outputBuilder.Add(new Output
                    { 
                        Text = Ui("🫡 Welcome to the CheckMade Bot. " +
                                  "I shall follow your command!")
                    });
                }
                
                if (await IsInputInterruptingPreviousWorkflowAsync(currentInput))
                {
                    outputBuilder.Add(
                        new Output
                        {
                            Text = Ui("FYI: you interrupted the previous workflow before its completion or " +
                                      "successful submission."),
                            UpdateExistingOutputMessageId = msgIdCache.GetLastMessageId(currentInput.Agent)
                        });
                }

                var enrichedCurrentInput = currentInput with
                {
                    WorkflowGuid = Guid.NewGuid()
                };

                if (outputBuilder.Count == 0)
                {
                    outputBuilder.Add(new Output
                    {
                        Text = Ui("Fake Test Output")
                    });
                }
                
                return (Option<Input>.Some(enrichedCurrentInput), 
                    (IReadOnlyCollection<Output>)outputBuilder);
            },
            static failure =>
            {
                var failureOutput = failure switch
                {
                    ExceptionWrapper exw => new Output { Text = UiNoTranslate(exw.Exception.Message) },
                    _ => new Output { Text = ((BusinessError)failure).Error }
                };

                return Task.FromResult<(Option<Input> EnrichedOriginalInput,
                    IReadOnlyCollection<Output> ResultingOutputs)>((
                    Option<Input>.None(), [failureOutput]
                ));
            });
    }

    private static bool IsStartCommand(Input currentInput) =>
        currentInput.InputType.Equals(InputType.CommandMessage)
        && currentInput.Details.BotCommandEnumCode.Equals(Start.CommandCode);
    
    private async Task<bool> IsInputInterruptingPreviousWorkflowAsync(Input currentInput)
    {
        if (currentInput.OriginatorRole.IsNone)
            return false;
        
        if (currentInput.InputType is not InputType.CommandMessage)
            return false;
        
        var previousWorkflowInputHistory = 
            (await workflowUtils.GetAllCurrentInteractiveAsync(currentInput.Agent, currentInput))
            .SkipLast(1) // Excluding the current BotCommand input
            .GetLatestRecordsUpTo(static input => 
                input.InputType.Equals(InputType.CommandMessage))
            .ToList();

        return previousWorkflowInputHistory.Count > 0 && 
               !IsWorkflowTerminated(previousWorkflowInputHistory);
    }
    
    private bool IsWorkflowTerminated(IReadOnlyCollection<Input> inputHistory)
    {
        return
            inputHistory.Any(i =>
                i.ResultantState.IsSome &&
                glossary.GetDtType(
                        i.ResultantState.GetValueOrThrow().InStateId)
                    .IsAssignableTo(typeof(IWorkflowStateTerminator)));
    }
}
