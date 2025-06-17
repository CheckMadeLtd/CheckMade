using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Global.LanguageSetting.States;

public interface ILanguageSettingSet : IWorkflowStateTerminator;

public sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;