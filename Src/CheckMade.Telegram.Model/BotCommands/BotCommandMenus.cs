// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IDictionary<SubmissionsBotCommands, BotCommand> SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, BotCommand>
        {
            { SubmissionsBotCommands.Problem, 
                new BotCommand("/problem", "Ein Problem melden ❗") },
            { SubmissionsBotCommands.Assessment, 
                new BotCommand("/bewertung", "Eine Bewertung vornehmen ⭐") },
            { SubmissionsBotCommands.Settings, 
                new BotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { SubmissionsBotCommands.Logout, 
                new BotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };
    
    public IDictionary<CommunicationsBotCommands, BotCommand> CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, BotCommand>
        {
            { CommunicationsBotCommands.Contact, 
                new BotCommand("/kontakt", "Kontakt aufnehmen 💬") },
            { CommunicationsBotCommands.Settings, 
                new BotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { CommunicationsBotCommands.Logout, 
                new BotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };

    public IDictionary<NotificationsBotCommands, BotCommand> NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, BotCommand>
        {
            { NotificationsBotCommands.Status, 
                new BotCommand("/status", "Aktueller Statusreport 📋") },
            { NotificationsBotCommands.Settings, 
                new BotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { NotificationsBotCommands.Logout, 
                new BotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };
}