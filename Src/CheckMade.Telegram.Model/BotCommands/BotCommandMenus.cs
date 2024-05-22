// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IDictionary<SubmissionsBotCommands, ModelBotCommand> SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, ModelBotCommand>
        {
            { SubmissionsBotCommands.Problem, 
                new ModelBotCommand("/problem", "Ein Problem melden ❗") },
            { SubmissionsBotCommands.Assessment, 
                new ModelBotCommand("/bewertung", "Eine Bewertung vornehmen ⭐") },
            { SubmissionsBotCommands.Settings, 
                new ModelBotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { SubmissionsBotCommands.Logout, 
                new ModelBotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };
    
    public IDictionary<CommunicationsBotCommands, ModelBotCommand> CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, ModelBotCommand>
        {
            { CommunicationsBotCommands.Contact, 
                new ModelBotCommand("/kontakt", "Kontakt aufnehmen 💬") },
            { CommunicationsBotCommands.Settings, 
                new ModelBotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { CommunicationsBotCommands.Logout, 
                new ModelBotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };

    public IDictionary<NotificationsBotCommands, ModelBotCommand> NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, ModelBotCommand>
        {
            { NotificationsBotCommands.Status, 
                new ModelBotCommand("/status", "Aktueller Statusreport 📋") },
            { NotificationsBotCommands.Settings, 
                new ModelBotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { NotificationsBotCommands.Logout, 
                new ModelBotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };
}