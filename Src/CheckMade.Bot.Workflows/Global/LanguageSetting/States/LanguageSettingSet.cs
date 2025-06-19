using CheckMade.Abstract.Domain.Interfaces.Bot.Logic;

namespace CheckMade.Bot.Workflows.Global.LanguageSetting.States;

public interface ILanguageSettingSet : IWorkflowStateTerminator;

public sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;