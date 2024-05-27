// ReSharper disable StringLiteralTypo

using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.BotCommands.DefinitionEnumsByBotType;

namespace CheckMade.Telegram.Model.BotCommands;

public record BotCommandMenus
{
    public IDictionary<SubmissionsBotCommands, IDictionary<LanguageCode, ModelBotCommand>> 
        SubmissionsBotCommandMenu { get; } = 
        new Dictionary<SubmissionsBotCommands, IDictionary<LanguageCode, ModelBotCommand>>
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
    
    public IDictionary<CommunicationsBotCommands, IDictionary<LanguageCode, ModelBotCommand>> 
        CommunicationsBotCommandMenu { get; } = 
        new Dictionary<CommunicationsBotCommands, IDictionary<LanguageCode, ModelBotCommand>>
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

    public IDictionary<NotificationsBotCommands, IDictionary<LanguageCode, ModelBotCommand>> 
        NotificationsBotCommandMenu { get; } = 
        new Dictionary<NotificationsBotCommands, IDictionary<LanguageCode, ModelBotCommand>>
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