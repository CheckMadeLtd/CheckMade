using System.Collections.Immutable;
using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.Common.Model.Utils;

    public record ControlPromptsUiStringProvider
    {
        private readonly ImmutableDictionary<ControlPromptsCallbackId, UiString>.Builder _promptsBuilder = 
            ImmutableDictionary.CreateBuilder<ControlPromptsCallbackId, UiString>();
        
        public IReadOnlyDictionary<ControlPromptsCallbackId, UiString> ByControlPromptCallbackId { get; }

        public ControlPromptsUiStringProvider()
        {
            AddPrompt(ControlPrompts.Back, Ui("🔙 Back"));
            AddPrompt(ControlPrompts.Cancel, Ui("❌ Cancel"));
            AddPrompt(ControlPrompts.Skip, Ui("⏭️ Skip"));
            AddPrompt(ControlPrompts.Save, Ui("💾 Save"));
            AddPrompt(ControlPrompts.Submit, Ui("📤 Submit"));
            AddPrompt(ControlPrompts.Review, Ui("📋 Review"));
            AddPrompt(ControlPrompts.Edit, Ui("✏️ Edit details"));
            AddPrompt(ControlPrompts.Wait, Ui("⏳ Wait..."));
            
            AddPrompt(ControlPrompts.No, Ui("🚫 No"));
            AddPrompt(ControlPrompts.Yes, Ui("✅ Yes"));
            AddPrompt(ControlPrompts.Maybe, Ui("❓ Maybe"));
            
            AddPrompt(ControlPrompts.Bad, Ui("👎 Bad"));
            AddPrompt(ControlPrompts.Ok, Ui("😐 Ok"));
            AddPrompt(ControlPrompts.Good, Ui("👍 Good"));

            ByControlPromptCallbackId = _promptsBuilder.ToImmutable();
        }
        
        private void AddPrompt(ControlPrompts prompt, UiString uiString) =>
            _promptsBuilder.Add(new ControlPromptsCallbackId((long)prompt), uiString);
    }
