namespace CheckMade.Abstract.Domain.Model.Bot.Categories;

// Explicitly assigned Enum codes here important: they are serialised in the messages history in the database!
// Fundamentally changing the semantics of a code would require migration of historic detail data
public enum CommunicationsBotCommands
{
    // Code '1' is reserved for '/start' command, which is not part of the menu however
    // Contact = 10,
    
    Settings = BotCommandMenus.GlobalBotCommandsCodeThreshold_90,
    Logout = OperationsBotCommands.Logout
}