namespace CheckMade.Common.Model.ChatBot.Input;

public record TlgInput(
     TlgClientPort ClientPort,
     TlgInputType InputType,
     TlgInputDetails Details);
     