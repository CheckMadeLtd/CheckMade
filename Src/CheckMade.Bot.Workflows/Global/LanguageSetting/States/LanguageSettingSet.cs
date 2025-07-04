using CheckMade.Core.ServiceInterfaces.Bot;

namespace CheckMade.Bot.Workflows.Global.LanguageSetting.States;

public interface ILanguageSettingSet : IWorkflowStateTerminator;

public sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;