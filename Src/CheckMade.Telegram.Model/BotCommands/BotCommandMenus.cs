// ReSharper disable StringLiteralTypo

using CheckMade.Common.LangExt;

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
                        LanguageCode.En, 
                        new ModelBotCommand(Ui("/problem"), Ui("Report a problem ❗"))
                    },
                    {
                        LanguageCode.De, 
                        new ModelBotCommand(Ui("/problem"), Ui("Ein Problem melden ❗"))
                    }
                }
            },
            { SubmissionsBotCommands.Assessment, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En, 
                        new ModelBotCommand(Ui("/assessment"), Ui("Submit an assessment ⭐"))
                    },
                    {
                        LanguageCode.De, 
                        new ModelBotCommand(Ui("/bewertung"), Ui("Eine Bewertung vornehmen ⭐"))
                    }
                }
            },
            { SubmissionsBotCommands.Settings, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/settings"), Ui("Change settings ⚙️"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/einstellungen"), Ui("Einstellungen ändern ⚙️"))
                    }
                } 
            },
            { SubmissionsBotCommands.Logout, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En, 
                        new ModelBotCommand(Ui("/logout"), Ui("Exit this chat in your current role 💨"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/ausloggen"), 
                            Ui("In Ihrer aktuellen Rolle diesen Chat verlassen 💨"))
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
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/contact"), Ui("Contact a colleague 💬"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/kontakt"), Ui("Mit einem Kollegen Kontakt aufnehmen 💬"))
                    }
                }},
            { CommunicationsBotCommands.Settings,
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/settings"), Ui("Change settings ⚙️"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/einstellungen"), Ui("Einstellungen ändern ⚙️"))
                    }
                }},
            { CommunicationsBotCommands.Logout, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/logout"), Ui("Exit this chat in your current role 💨"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/ausloggen"), 
                            Ui("In Ihrer aktuellen Rolle diesen Chat verlassen 💨"))
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
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/status"), Ui("Current status report 📋"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/status"), Ui("Aktueller Statusreport 📋"))
                    }
                }},
            { NotificationsBotCommands.Settings, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/settings"), Ui("Change settings ⚙️"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/einstellungen"), Ui("Einstellungen ändern ⚙️"))
                    }
                }},
            { NotificationsBotCommands.Logout, 
                new Dictionary<LanguageCode, ModelBotCommand>
                {
                    {
                        LanguageCode.En,
                        new ModelBotCommand(Ui("/logout"), Ui("Exit this chat in your current role 💨"))
                    },
                    {
                        LanguageCode.De,
                        new ModelBotCommand(Ui("/ausloggen"), 
                            Ui("In Ihrer aktuellen Rolle diesen Chat verlassen 💨"))
                    }
                }}
        };
}