// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record SubmissionsBotCommandMenu
{
    public IDictionary<SubmissionsBotCommands, BotCommand> Menu { get; init; } = 
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
}