namespace CheckMade.Telegram.Model.BotCommands;

// Explicitly assigned Enum codes here important: they are serialised in the messages history in the database!
// Fundamentally changing the semantics of a code would require migration of historic detail data
public enum SubmissionsBotCommands
{
    Problem = 10,
    Assessment = 20,
    Settings = 90,
    Logout = 99
}