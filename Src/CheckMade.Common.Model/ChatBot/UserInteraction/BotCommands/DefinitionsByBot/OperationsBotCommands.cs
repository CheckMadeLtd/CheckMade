namespace CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;

// Explicitly assigned Enum codes here important: they are serialised in the messages history in the database!
// Fundamentally changing the semantics of a code would require migration of historic detail data
public enum OperationsBotCommands
{
    // Code '1' is reserved for '/start' command, which is not part of the menu however
    NewIssue = 10,
    NewAssessment = 20,
    
    Settings = BotCommandMenus.SameBotCommandSemanticsThreshold_90,
    Logout = 99
}