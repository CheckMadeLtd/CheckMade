namespace CheckMade.Common.Model.ChatBot.Input;

public record TlgInput(
     TlgAgent ClientPort,
     TlgInputType InputType,
     TlgInputDetails Details);
     