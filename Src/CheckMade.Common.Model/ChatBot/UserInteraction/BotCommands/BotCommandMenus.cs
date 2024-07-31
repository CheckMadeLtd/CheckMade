// ReSharper disable StringLiteralTypo

using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;

public sealed record BotCommandMenus
{
    public const int SameBotCommandSemanticsThreshold_90 = 90;
    
    public IReadOnlyDictionary<OperationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> 
        OperationsBotCommandMenu { get; } = 
        new Dictionary<OperationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>>
        {
            { OperationsBotCommands.NewIssue, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TlgBotCommand("/issue", "❗ Report a new issue")
                    },
                    {
                        LanguageCode.de, 
                        new TlgBotCommand("/problem", "❗ Ein neues Problem melden")
                    }
                }
            },
            { OperationsBotCommands.NewAssessment, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TlgBotCommand("/assessment", "⭐ Submit a new assessment")
                    },
                    {
                        LanguageCode.de, 
                        new TlgBotCommand("/bewertung", "⭐ Eine neue Bewertung vornehmen")
                    }
                }
            },
            { OperationsBotCommands.Settings, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/settings", "⚙️ Change language setting")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/einstellungen", "⚙️ Spracheinstellung ändern")
                    }
                } 
            },
            { OperationsBotCommands.Logout, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TlgBotCommand("/logout", "💨 Exit this chat in your current role")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/ausloggen", 
                            "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
                    }
                } 
            }
        };
    
    public IReadOnlyDictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> 
        CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>>
        {
            { CommunicationsBotCommands.Contact, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/contact", "💬 Contact a colleague")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/kontakt", "💬 Mit einem Kollegen Kontakt aufnehmen")
                    }
                }},
            { CommunicationsBotCommands.Settings,
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/settings", "⚙️ Change language setting")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/einstellungen", "⚙️ Spracheinstellung ändern")
                    }
                }},
            { CommunicationsBotCommands.Logout, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/logout", "💨 Exit this chat in your current role")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/ausloggen", 
                            "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
                    }
                }}
        };

    public IReadOnlyDictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> 
        NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, TlgBotCommand>>
        {
            { NotificationsBotCommands.Status, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/status", "📋 Current status report")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/status", "📋 Aktueller Statusreport")
                    }
                }},
            { NotificationsBotCommands.Settings, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/settings", "⚙️ Change language setting")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/einstellungen", "⚙️ Spracheinstellung ändern")
                    }
                }},
            { NotificationsBotCommands.Logout, 
                new Dictionary<LanguageCode, TlgBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TlgBotCommand("/logout", "💨 Exit this chat in your current role")
                    },
                    {
                        LanguageCode.de,
                        new TlgBotCommand("/ausloggen", 
                            "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
                    }
                }}
        };
}