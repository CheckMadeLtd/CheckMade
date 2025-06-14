using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;

internal interface ILanguageSettingSet : IWorkflowStateTerminator;

internal sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;