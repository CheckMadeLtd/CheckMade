using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;

internal interface ILanguageSettingSet : IWorkflowStateTerminator;

internal sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;