// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IDictionary<SubmissionsBotCommands, ModelBotCommand> SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, ModelBotCommand>
        {
            { SubmissionsBotCommands.Problem, 
                new ModelBotCommand(Ui("/problem"), Ui("Ein Problem melden ❗")) },
            { SubmissionsBotCommands.Assessment, 
                new ModelBotCommand(Ui("/bewertung"), Ui("Eine Bewertung vornehmen ⭐")) },
            { SubmissionsBotCommands.Settings, 
                new ModelBotCommand(Ui("/einstellungen"), Ui("Einstellungen ändern ⚙️")) },
            { SubmissionsBotCommands.Logout, 
                new ModelBotCommand(Ui("/ausloggen"), Ui("Aktuelle Rolle von diesem Chat trennen 💨")) }
        };
    
    public IDictionary<CommunicationsBotCommands, ModelBotCommand> CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, ModelBotCommand>
        {
            { CommunicationsBotCommands.Contact, 
                new ModelBotCommand(Ui("/kontakt"), Ui("Kontakt aufnehmen 💬")) },
            { CommunicationsBotCommands.Settings, 
                new ModelBotCommand(Ui("/einstellungen"), Ui("Einstellungen ändern ⚙️")) },
            { CommunicationsBotCommands.Logout, 
                new ModelBotCommand(Ui("/ausloggen"), Ui("Aktuelle Rolle von diesem Chat trennen 💨")) }
        };

    public IDictionary<NotificationsBotCommands, ModelBotCommand> NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, ModelBotCommand>
        {
            { NotificationsBotCommands.Status, 
                new ModelBotCommand(Ui("/status"), Ui("Aktueller Statusreport 📋")) },
            { NotificationsBotCommands.Settings, 
                new ModelBotCommand(Ui("/einstellungen"), Ui("Einstellungen ändern ⚙️")) },
            { NotificationsBotCommands.Logout, 
                new ModelBotCommand(Ui("/ausloggen"), Ui("Aktuelle Rolle von diesem Chat trennen 💨")) }
        };
}