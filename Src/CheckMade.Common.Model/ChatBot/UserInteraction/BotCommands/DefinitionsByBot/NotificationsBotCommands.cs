namespace CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;

// Explicitly assigned Enum codes here important: they are serialised in the messages history in the database!
// Fundamentally changing the semantics of a code would require migration of historic detail data
public enum NotificationsBotCommands
{
    // Code '1' is reserved for '/start' command, which is not part of the menu however
    Status = 10,
    
    Settings = BotCommandMenus.SameBotCommandSemanticsThreshold_90,
    Logout = OperationsBotCommands.Logout
}