﻿using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.Utils;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<(Option<TlgInput> EnrichedOriginalInput, IReadOnlyCollection<OutputDto> ResultingOutputs)> 
        ProcessInputAsync(Result<TlgInput> input);
}

internal sealed class InputProcessor(
    IWorkflowIdentifier workflowIdentifier,
    IGeneralWorkflowUtils workflowUtils,
    IDomainGlossary glossary,
    ILogger<InputProcessor> logger)
    : IInputProcessor
{
    public async Task<(Option<TlgInput> EnrichedOriginalInput, IReadOnlyCollection<OutputDto> ResultingOutputs)> 
        ProcessInputAsync(Result<TlgInput> input)
    {
        return await input.Match(
            async currentInput =>
            {
                if (currentInput.InputType == TlgInputType.Location)
                {
                    return (currentInput, []);
                }
                
                List<OutputDto> outputBuilder = [];

                if (IsStartCommand(currentInput))
                {
                    outputBuilder.Add(new OutputDto
                    { 
                        Text = Ui("🫡 Welcome to the CheckMade ChatBot. " +
                                  "I shall follow your command!")
                    });
                }
                
                if (await IsInputInterruptingPreviousWorkflowAsync(currentInput))
                {
                    outputBuilder.Add(
                        new OutputDto
                        {
                            Text = Ui("FYI: you interrupted the previous workflow before its completion or " +
                                      "successful submission."),
                            UpdateExistingOutputMessageId = new TlgMessageId(currentInput.TlgMessageId - 1)
                        });
                }

                var inputHistory = 
                    await workflowUtils.GetAllCurrentInteractiveAsync(currentInput.TlgAgent, currentInput);
                
                var activeWorkflow = 
                    await workflowIdentifier.IdentifyAsync(inputHistory);
                
                var responseResult = 
                    await GetResponseFromActiveWorkflowAsync(activeWorkflow, currentInput);

                var enrichedCurrentInput = currentInput with
                {
                    ResultantWorkflow = GetResultantWorkflowState(responseResult, activeWorkflow)
                                        ?? Option<ResultantWorkflowState>.None(),
                    EntityGuid = GetEntityGuid(responseResult)
                };
                
                return (
                    Option<TlgInput>.Some(enrichedCurrentInput), 
                    ResolveResponseResultIntoOutputs(
                        responseResult,
                        outputBuilder,
                        activeWorkflow,
                        currentInput));
            },
            static failure =>
            {
                var failureOutput = failure switch
                {
                    ExceptionWrapper exw => new OutputDto { Text = UiNoTranslate(exw.Exception.Message) },
                    _ => new OutputDto { Text = ((BusinessError)failure).Error }
                };

                return Task.FromResult<(Option<TlgInput> EnrichedOriginalInput,
                    IReadOnlyCollection<OutputDto> ResultingOutputs)>((
                    Option<TlgInput>.None(), [failureOutput]
                ));
            });
    }

    private static bool IsStartCommand(TlgInput currentInput) =>
        currentInput.InputType.Equals(TlgInputType.CommandMessage)
        && currentInput.Details.BotCommandEnumCode.Equals(TlgStart.CommandCode);
    
    private ResultantWorkflowState? GetResultantWorkflowState(
        Result<WorkflowResponse> response,
        Option<WorkflowBase> activeWorkflow)
    {
        ResultantWorkflowState? workflowInfo = null;
                
        var newState = response.Match(
            static r => r.NewStateId,
            static _ => Option<string>.None());
                
        if (activeWorkflow.IsSome && newState.IsSome)
        {
            workflowInfo = new ResultantWorkflowState(
                glossary.GetId(activeWorkflow.GetValueOrThrow().GetType()),
                newState.GetValueOrThrow());
        }

        return workflowInfo;
    }

    private static Option<Guid> GetEntityGuid(Result<WorkflowResponse> response) =>
        response.Match(
            static r => r.EntityGuid,
            static _ => Option<Guid>.None());
    
    private async Task<bool> IsInputInterruptingPreviousWorkflowAsync(TlgInput currentInput)
    {
        if (currentInput.OriginatorRole.IsNone)
            return false;
        
        if (currentInput.InputType is not TlgInputType.CommandMessage)
            return false;
        
        var previousWorkflowInputHistory = 
            (await workflowUtils.GetAllCurrentInteractiveAsync(currentInput.TlgAgent, currentInput))
            .SkipLast(1) // Excluding the current BotCommand input
            .GetLatestRecordsUpTo(static input => 
                input.InputType.Equals(TlgInputType.CommandMessage))
            .ToList();

        return previousWorkflowInputHistory.Count > 0 && 
               !IsWorkflowTerminated(previousWorkflowInputHistory);
    }
    
    private bool IsWorkflowTerminated(IReadOnlyCollection<TlgInput> inputHistory)
    {
        return
            inputHistory.Any(i =>
                i.ResultantWorkflow.IsSome &&
                glossary.GetDtType(
                        i.ResultantWorkflow.GetValueOrThrow().InStateId)
                    .IsAssignableTo(typeof(IWorkflowStateTerminator)));
    }

    private static async Task<Result<WorkflowResponse>>
        GetResponseFromActiveWorkflowAsync(
            Option<WorkflowBase> activeWorkflow,
            TlgInput currentInput)
    {
        return await activeWorkflow.Match(
            wf => 
                wf.GetResponseAsync(currentInput),
            static () => 
                Task.FromResult(Result<WorkflowResponse>
                    .Succeed(new WorkflowResponse(
                        [
                            new OutputDto 
                            { 
                                Text = UiConcatenate(
                                    Ui("Your input can't be processed."),
                                    UiNewLines(1),
                                    IInputProcessor.SeeValidBotCommandsInstruction)
                            }
                        ], 
                        Option<string>.None(),
                        Option<Guid>.None()))));
    }

    private IReadOnlyCollection<OutputDto> ResolveResponseResultIntoOutputs(
        Result<WorkflowResponse> responseResult,
        List<OutputDto> outputBuilder,
        Option<WorkflowBase> activeWorkflow,
        TlgInput currentInput)
    {
        return responseResult.Match(
            wr => 
            {
                outputBuilder.AddRange(wr.Output);
                return outputBuilder;
            },
            failure =>
            {
                switch (failure)
                {
                    case ExceptionWrapper exw:
                        logger.LogError($"""
                                         The workflow '{activeWorkflow.GetValueOrDefault().GetType()}' has returned
                                         this exception: '{exw.Exception.Message}'. Next, the corresponding input parameters.
                                         UserId: {currentInput.TlgAgent.UserId}; ChatId: {currentInput.TlgAgent.ChatId}; 
                                         InputType: {currentInput.InputType}; InteractionMode: {currentInput.TlgAgent.Mode};
                                         Date: {currentInput.TlgDate}; 
                                         For more details of input, check database!
                                         """);
                        throw exw.Exception;
                    
                    default:
                        return [new OutputDto { Text = ((BusinessError)failure).Error }];
                }
            }
        );
    }
}
