// ReSharper disable StringLiteralTypo

using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.BotCommands.DefinitionsByBotType;

namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IReadOnlyDictionary<SubmissionsBotCommands, IReadOnlyDictionary<LanguageCode, ModelBotCommand>> 
        SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, IReadOnlyDictionary<LanguageCode, ModelBotCommand>>
        {
            { SubmissionsBotCommands.Problem, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new ModelBotCommand("/problem", "Report a problem ❗")
                    },
                    {
                        LanguageCode.de, 
                        new ModelBotCommand("/problem", "Ein Problem melden ❗")
                    }
                }
            },
            { SubmissionsBotCommands.Assessment, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new ModelBotCommand("/assessment", "Submit an assessment ⭐")
                    },
                    {
                        LanguageCode.de, 
                        new ModelBotCommand("/bewertung", "Eine Bewertung vornehmen ⭐")
                    }
                }
            },
            { SubmissionsBotCommands.Settings, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/settings", "Change settings ⚙️")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/einstellungen", "Einstellungen ändern ⚙️")
                    }
                } 
            },
            { SubmissionsBotCommands.Logout, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en, 
                        new ModelBotCommand("/logout", "Exit this chat in your current role 💨")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/ausloggen", 
                            "In Ihrer aktuellen Rolle diesen Chat verlassen 💨")
                    }
                } 
            }
        };
    
    public IReadOnlyDictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, ModelBotCommand>> 
        CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, IReadOnlyDictionary<LanguageCode, ModelBotCommand>>
        {
            { CommunicationsBotCommands.Contact, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/contact", "Contact a colleague 💬")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/kontakt", "Mit einem Kollegen Kontakt aufnehmen 💬")
                    }
                }},
            { CommunicationsBotCommands.Settings,
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/settings", "Change settings ⚙️")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/einstellungen", "Einstellungen ändern ⚙️")
                    }
                }},
            { CommunicationsBotCommands.Logout, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/logout", "Exit this chat in your current role 💨")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/ausloggen", 
                            "In Ihrer aktuellen Rolle diesen Chat verlassen 💨")
                    }
                }}
        };

    public IReadOnlyDictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, ModelBotCommand>> 
        NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, IReadOnlyDictionary<LanguageCode, ModelBotCommand>>
        {
            { NotificationsBotCommands.Status, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/status", "Current status report 📋")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/status", "Aktueller Statusreport 📋")
                    }
                }},
            { NotificationsBotCommands.Settings, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/settings", "Change settings ⚙️")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/einstellungen", "Einstellungen ändern ⚙️")
                    }
                }},
            { NotificationsBotCommands.Logout, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.en,
                        new ModelBotCommand("/logout", "Exit this chat in your current role 💨")
                    },
                    {
                        LanguageCode.de,
                        new ModelBotCommand("/ausloggen", 
                            "In Ihrer aktuellen Rolle diesen Chat verlassen 💨")
                    }
                }}
        };
}