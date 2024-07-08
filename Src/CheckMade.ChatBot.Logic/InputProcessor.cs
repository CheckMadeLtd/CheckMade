using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> input);
}

internal class InputProcessor(
        IWorkflowIdentifier workflowIdentifier,
        ITlgInputsRepository inputsRepo,
        ILogicUtils logicUtils,
        IDomainGlossary glossary,
        ILogger<InputProcessor> logger)
    : IInputProcessor
{
    public async Task<IReadOnlyCollection<OutputDto>> ProcessInputAsync(Result<TlgInput> input)
    {
        return await input.Match(
            async currentInput =>
            {
                if (currentInput.InputType == TlgInputType.Location)
                {
                    await inputsRepo.AddAsync(currentInput);
                    
                    return [];
                }
                
                List<OutputDto> responseBuilder = [];

                if (currentInput.InputType.Equals(TlgInputType.CommandMessage)
                    && currentInput.Details.BotCommandEnumCode.Equals(TlgStart.CommandCode))
                {
                    responseBuilder.Add(new OutputDto{ Text = Ui("🫡 Welcome to the CheckMade ChatBot. " +
                                                               "I shall follow your command!") });
                }
                
                if (await IsInputInterruptingPreviousWorkflowAsync(currentInput))
                {
                    responseBuilder.Add(new OutputDto
                        {
                            Text = Ui("FYI: you interrupted the previous workflow before its completion or " +
                                      "successful submission.")
                        });
                }

                var activeWorkflowInputHistory = 
                    await logicUtils.GetInteractiveSinceLastBotCommand(currentInput.TlgAgent);
                
                if (IsCurrentInputFromOutOfScopeWorkflow(currentInput, activeWorkflowInputHistory))
                {
                    await inputsRepo.AddAsync(currentInput);
                    
                    return 
                        [new OutputDto 
                        {
                            Text = Ui("The previous workflow was completed, " +
                                      "so your last message/action will be ignored.") 
                        }];
                }

                var activeWorkflow = workflowIdentifier.Identify(activeWorkflowInputHistory);
                
                var response = await activeWorkflow.Match(
                    wf => 
                        wf.GetResponseAsync(currentInput),
                    () => 
                        Task.FromResult(Result<IReadOnlyCollection<OutputDto>>.FromSuccess(
                            [new OutputDto 
                            { 
                                Text = Ui("My placeholder answer for lack of a workflow handling your input."), 
                            }])
                        ));

                await inputsRepo.AddAsync(currentInput with
                {
                    ResultantWorkflow = new ResultantWorkflowInfo(
                        glossary.IdAndUiByTerm[Dt(activeWorkflow.GetType())].callbackId.Id,
                        // ToDo: probably save the State that was determined in each Workflow class in a private field and expose via GetProperty? OO now!
                        // So we avoid recalculating it, in case e.g. DetermineCurrentState() will require DB access in the future in some Workflows.
                        // (at this point it already was calculated from the GetResponseAsync call so it feels natural to save it in the instance of the Workflow)
                });
                
                return response.Match(
                    outputs => 
                    {
                        responseBuilder.AddRange(outputs);
                        
                        return 
                            responseBuilder
                            .ToImmutableReadOnlyCollection();
                    },
                    error =>
                    {
                        logger.LogWarning($"""
                                           The workflow '{activeWorkflow.GetValueOrDefault().GetType()}' has returned
                                           this Error Result: '{error}'. Next, the corresponding input parameters.
                                           UserId: {currentInput.TlgAgent.UserId}; ChatId: {currentInput.TlgAgent.ChatId}; 
                                           InputType: {currentInput.InputType}; InteractionMode: {currentInput.TlgAgent.Mode};
                                           Date: {currentInput.Details.TlgDate}; 
                                           For more details of input, check database!
                                           """);
                        
                        return 
                            [new OutputDto { Text = error }];
                    }
                );
            },
            // This error was already logged at its source, in ToModelConverter
            error => 
                Task.FromResult<IReadOnlyCollection<OutputDto>>(
                    [new OutputDto { Text = error }]));
    }

    private async Task<bool> IsInputInterruptingPreviousWorkflowAsync(TlgInput currentInput)
    {
        if (currentInput.OriginatorRole.IsNone)
            return false;
        
        if (currentInput.InputType is not TlgInputType.CommandMessage)
            return false;
        
        var previousWorkflowInputHistory = 
            (await logicUtils.GetAllCurrentInteractiveAsync(currentInput.TlgAgent))
            .SkipLast(1) // Excluding the current BotCommand input
            .GetLatestRecordsUpTo(input => 
                input.InputType.Equals(TlgInputType.CommandMessage))
            .ToImmutableReadOnlyCollection();

        var previousWorkflow = workflowIdentifier.Identify(previousWorkflowInputHistory);

        return previousWorkflowInputHistory.Count > 0 && 
               previousWorkflow.IsSome && 
               !previousWorkflow.GetValueOrThrow().IsCompleted(previousWorkflowInputHistory);
    }

    private static bool IsCurrentInputFromOutOfScopeWorkflow(
        TlgInput currentInput, IReadOnlyCollection<TlgInput> activeWorkflowInputHistory) => 
        ILogicUtils.GetLastBotCommand(activeWorkflowInputHistory).Match(
            lastBotCommand => 
            {
                var activeWorkflowStartMessageId = lastBotCommand.Details.TlgMessageId;
                
                return currentInput.Details.TlgMessageId < activeWorkflowStartMessageId;
            },
            () => false);
}
