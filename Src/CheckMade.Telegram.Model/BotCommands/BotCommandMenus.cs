// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IDictionary<SubmissionsBotCommands, ModelBotCommand> SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, ModelBotCommand>
        {
            { SubmissionsBotCommands.Problem, 
                new ModelBotCommand(UiSm("/problem"), UiSm("Ein Problem melden ❗")) },
            { SubmissionsBotCommands.Assessment, 
                new ModelBotCommand(UiSm("/bewertung"), UiSm("Eine Bewertung vornehmen ⭐")) },
            { SubmissionsBotCommands.Settings, 
                new ModelBotCommand(UiSm("/einstellungen"), UiSm("Einstellungen ändern ⚙️")) },
            { SubmissionsBotCommands.Logout, 
                new ModelBotCommand(UiSm("/ausloggen"), UiSm("Aktuelle Rolle von diesem Chat trennen 💨")) }
        };
    
    public IDictionary<CommunicationsBotCommands, ModelBotCommand> CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, ModelBotCommand>
        {
            { CommunicationsBotCommands.Contact, 
                new ModelBotCommand(UiSm("/kontakt"), UiSm("Kontakt aufnehmen 💬")) },
            { CommunicationsBotCommands.Settings, 
                new ModelBotCommand(UiSm("/einstellungen"), UiSm("Einstellungen ändern ⚙️")) },
            { CommunicationsBotCommands.Logout, 
                new ModelBotCommand(UiSm("/ausloggen"), UiSm("Aktuelle Rolle von diesem Chat trennen 💨")) }
        };

    public IDictionary<NotificationsBotCommands, ModelBotCommand> NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, ModelBotCommand>
        {
            { NotificationsBotCommands.Status, 
                new ModelBotCommand(UiSm("/status"), UiSm("Aktueller Statusreport 📋")) },
            { NotificationsBotCommands.Settings, 
                new ModelBotCommand(UiSm("/einstellungen"), UiSm("Einstellungen ändern ⚙️")) },
            { NotificationsBotCommands.Logout, 
                new ModelBotCommand(UiSm("/ausloggen"), UiSm("Aktuelle Rolle von diesem Chat trennen 💨")) }
        };
}