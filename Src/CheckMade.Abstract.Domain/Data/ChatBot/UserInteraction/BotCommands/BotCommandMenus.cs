// ReSharper disable StringLiteralTypo

using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands;

public sealed record BotCommandMenus
{
    public const int GlobalBotCommandsCodeThreshold_90 = 90;
    
    public IReadOnlyDictionary<OperationsBotCommands, IReadOnlyDictionary<LanguageCode, BotCommand>> 
        OperationsBotCommandMenu { get; } = 
        new Dictionary<OperationsBotCommands, IReadOnlyDictionary<LanguageCode, BotCommand>>
        {
            [OperationsBotCommands.NewSubmission] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/submission", "❗ New submission"),
                [LanguageCode.de] = new("/meldung", "❗ Neue Meldung")
            },
            [OperationsBotCommands.Settings] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/settings", "⚙️ Change language setting"),
                [LanguageCode.de] = new("/einstellungen", "⚙️ Spracheinstellung ändern")
            },
            [OperationsBotCommands.Logout] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/logout", "💨 Exit this chat in your current role"),
                [LanguageCode.de] = new("/ausloggen", "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
            } 
        };
    
    public IReadOnlyDictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, BotCommand>> 
        CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, BotCommand>>
        {
            // [CommunicationsBotCommands.Contact] = new Dictionary<LanguageCode, BotCommand>
            // {
            //     [LanguageCode.en] = new("/contact", "💬 Contact a colleague"),
            //     [LanguageCode.de] = new("/kontakt", "💬 Mit einem Kollegen Kontakt aufnehmen")
            // },
            [CommunicationsBotCommands.Settings] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/settings", "⚙️ Change language setting"),
                [LanguageCode.de] = new("/einstellungen", "⚙️ Spracheinstellung ändern")
            },
            [CommunicationsBotCommands.Logout] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/logout", "💨 Exit this chat in your current role"),
                [LanguageCode.de] = new("/ausloggen", "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
            }
        };

    public IReadOnlyDictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, BotCommand>> 
        NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, BotCommand>>
        {
            // [NotificationsBotCommands.Status] = new Dictionary<LanguageCode, BotCommand>
            // {
            //     [LanguageCode.en] = new("/status", "📋 Current status report"),
            //     [LanguageCode.de] = new("/status", "📋 Aktueller Statusreport")
            // },
            [NotificationsBotCommands.Settings] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/settings", "⚙️ Change language setting"),
                [LanguageCode.de] = new("/einstellungen", "⚙️ Spracheinstellung ändern")
            },
            [NotificationsBotCommands.Logout] = new Dictionary<LanguageCode, BotCommand>
            {
                [LanguageCode.en] = new("/logout", "💨 Exit this chat in your current role"),
                [LanguageCode.de] = new("/ausloggen", "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
            }
        };
}