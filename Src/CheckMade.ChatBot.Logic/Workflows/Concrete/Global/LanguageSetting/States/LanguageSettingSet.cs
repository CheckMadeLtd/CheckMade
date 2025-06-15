using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;

internal interface ILanguageSettingSet : IWorkflowStateTerminator;

internal sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;