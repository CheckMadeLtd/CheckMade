// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IDictionary<SubmissionsBotCommands, ModelBotCommand> SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, ModelBotCommand>
        {
            { SubmissionsBotCommands.Problem, 
                new ModelBotCommand(Ui("/problem"), Ui("Report a problem ❗")) },
            { SubmissionsBotCommands.Assessment, 
                new ModelBotCommand(Ui("/assessment"), Ui("Submit an assessment ⭐")) },
            { SubmissionsBotCommands.Settings, 
                new ModelBotCommand(Ui("/settings"), Ui("Change settings ⚙️")) },
            { SubmissionsBotCommands.Logout, 
                new ModelBotCommand(Ui("/logout"), Ui("Exit this chat in your current role 💨")) }
        };
    
    public IDictionary<CommunicationsBotCommands, ModelBotCommand> CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, ModelBotCommand>
        {
            { CommunicationsBotCommands.Contact, 
                new ModelBotCommand(Ui("/contact"), Ui("Contact a colleague 💬")) },
            { CommunicationsBotCommands.Settings, 
                new ModelBotCommand(Ui("/settings"), Ui("Change settings ⚙️")) },
            { CommunicationsBotCommands.Logout, 
                new ModelBotCommand(Ui("/logout"), Ui("Exit this chat in your current role 💨")) }
        };

    public IDictionary<NotificationsBotCommands, ModelBotCommand> NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, ModelBotCommand>
        {
            { NotificationsBotCommands.Status, 
                new ModelBotCommand(Ui("/status"), Ui("Aktueller Statusreport 📋")) },
            { NotificationsBotCommands.Settings, 
                new ModelBotCommand(Ui("/settings"), Ui("Change settings ⚙️")) },
            { NotificationsBotCommands.Logout, 
                new ModelBotCommand(Ui("/logout"), Ui("Exit this chat in your current role 💨")) }
        };
}