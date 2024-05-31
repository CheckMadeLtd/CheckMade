// ReSharper disable StringLiteralTypo

using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;

namespace CheckMade.Telegram.Model.BotCommand;

public record BotCommandMenus
{
    public IReadOnlyDictionary<SubmissionsBotCommands, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>> 
        SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>>
        {
            { SubmissionsBotCommands.NewIssue, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TelegramBotCommand("/issue", "❗ Report a new issue")
                    },
                    {
                        LanguageCode.de, 
                        new TelegramBotCommand("/problem", "❗ Ein neues Problem melden")
                    }
                }
            },
            { SubmissionsBotCommands.NewAssessment, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TelegramBotCommand("/assessment", "⭐ Submit a new assessment")
                    },
                    {
                        LanguageCode.de, 
                        new TelegramBotCommand("/bewertung", "⭐ Eine neue Bewertung vornehmen")
                    }
                }
            },
            { SubmissionsBotCommands.Settings, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/settings", "⚙️ Change settings")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/einstellungen", "⚙️ Einstellungen ändern")
                    }
                } 
            },
            { SubmissionsBotCommands.Experimental, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TelegramBotCommand("/experiment", "A Experiment")
                    },
                    {
                        LanguageCode.de, 
                        new TelegramBotCommand("/experiment", "Ein Experiment")
                    }
                } 
            },
            { SubmissionsBotCommands.Logout, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new TelegramBotCommand("/logout", "💨 Exit this chat in your current role")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/ausloggen", 
                            "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
                    }
                } 
            }
        };
    
    public IReadOnlyDictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>> 
        CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>>
        {
            { CommunicationsBotCommands.Contact, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/contact", "💬 Contact a colleague")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/kontakt", "💬 Mit einem Kollegen Kontakt aufnehmen")
                    }
                }},
            { CommunicationsBotCommands.Settings,
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/settings", "⚙️ Change settings")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/einstellungen", "⚙️ Einstellungen ändern")
                    }
                }},
            { CommunicationsBotCommands.Logout, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/logout", "💨 Exit this chat in your current role")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/ausloggen", 
                            "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
                    }
                }}
        };

    public IReadOnlyDictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>> 
        NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>>
        {
            { NotificationsBotCommands.Status, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/status", "📋 Current status report")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/status", "📋 Aktueller Statusreport")
                    }
                }},
            { NotificationsBotCommands.Settings, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/settings", "⚙️ Change settings")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/einstellungen", "⚙️ Einstellungen ändern")
                    }
                }},
            { NotificationsBotCommands.Logout, 
                new Dictionary<LanguageCode, TelegramBotCommand>
                {
                    {
                        LanguageCode.en,
                        new TelegramBotCommand("/logout", "💨 Exit this chat in your current role")
                    },
                    {
                        LanguageCode.de,
                        new TelegramBotCommand("/ausloggen", 
                            "💨 In Ihrer aktuellen Rolle diesen Chat verlassen")
                    }
                }}
        };
}