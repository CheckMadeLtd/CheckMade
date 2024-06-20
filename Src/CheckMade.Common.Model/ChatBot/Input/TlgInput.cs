namespace CheckMade.Common.Model.ChatBot.Input;

public record TlgInput(
     TlgAgent TlgAgent,
     TlgInputType InputType,
     TlgInputDetails Details);
     