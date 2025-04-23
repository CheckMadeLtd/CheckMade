// ReSharper disable StringLiteralTypo

using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;

public sealed record BotCommandMenus
{
    public const int GlobalBotCommandsCodeThreshold_90 = 90;
    
    public IReadOnlyDictionary<OperationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> 
        OperationsBotCommandMenu { get; } = 
        new Dictionary<OperationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>>
        {
            [OperationsBotCommands.NewIssue] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/issue", "❗ Report a new issue"),
                [LanguageCode.de] = new("/problem", "❗ Ein neues Problem melden")
            },
            [OperationsBotCommands.Settings] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/settings", "⚙️ Change language setting"),
                [LanguageCode.de] = new("/einstellungen", "⚙️ Spracheinstellung ändern")
            },
            [OperationsBotCommands.Logout] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/logout", "💨 Exit this chat in your current role"),
                [LanguageCode.de] = new("/ausloggen", "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
            } 
        };
    
    public IReadOnlyDictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> 
        CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>>
        {
            // [CommunicationsBotCommands.Contact] = new Dictionary<LanguageCode, TlgBotCommand>
            // {
            //     [LanguageCode.en] = new("/contact", "💬 Contact a colleague"),
            //     [LanguageCode.de] = new("/kontakt", "💬 Mit einem Kollegen Kontakt aufnehmen")
            // },
            [CommunicationsBotCommands.Settings] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/settings", "⚙️ Change language setting"),
                [LanguageCode.de] = new("/einstellungen", "⚙️ Spracheinstellung ändern")
            },
            [CommunicationsBotCommands.Logout] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/logout", "💨 Exit this chat in your current role"),
                [LanguageCode.de] = new("/ausloggen", "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
            }
        };

    public IReadOnlyDictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> 
        NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>>
        {
            // [NotificationsBotCommands.Status] = new Dictionary<LanguageCode, TlgBotCommand>
            // {
            //     [LanguageCode.en] = new("/status", "📋 Current status report"),
            //     [LanguageCode.de] = new("/status", "📋 Aktueller Statusreport")
            // },
            [NotificationsBotCommands.Settings] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/settings", "⚙️ Change language setting"),
                [LanguageCode.de] = new("/einstellungen", "⚙️ Spracheinstellung ändern")
            },
            [NotificationsBotCommands.Logout] = new Dictionary<LanguageCode, TlgBotCommand>
            {
                [LanguageCode.en] = new("/logout", "💨 Exit this chat in your current role"),
                [LanguageCode.de] = new("/ausloggen", "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
            }
        };
}