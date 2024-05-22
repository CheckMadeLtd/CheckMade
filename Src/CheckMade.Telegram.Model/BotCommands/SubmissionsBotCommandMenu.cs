// ReSharper disable StringLiteralTypo
namespace CheckMade.Telegram.Model.BotCommands;

public record SubmissionsBotCommandMenu
{
    public IDictionary<SubmissionsBotCommands, BotCommand> Menu { get; init; } = 
        new Dictionary<SubmissionsBotCommands, BotCommand>
        {
            { SubmissionsBotCommands.Problem, 
                new BotCommand("/problem", "Ein Problem melden ❗") },
            { SubmissionsBotCommands.Bewertung, 
                new BotCommand("/bewertung", "Eine Bewertung vornehmen ⭐") },
            { SubmissionsBotCommands.Einstellungen, 
                new BotCommand("/einstellungen", "Einstellungen ändern ⚙️") },
            { SubmissionsBotCommands.Ausloggen, 
                new BotCommand("/ausloggen", "Aktuelle Rolle von diesem Chat trennen 💨") }
        };
}