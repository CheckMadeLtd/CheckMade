using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;

internal interface ILanguageSettingSet : IWorkflowStateTerminator;

internal sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;