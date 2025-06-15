using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Global.LanguageSetting.States;

internal interface ILanguageSettingSet : IWorkflowStateTerminator;

internal sealed record LanguageSettingSet(IDomainGlossary Glossary) : ILanguageSettingSet;