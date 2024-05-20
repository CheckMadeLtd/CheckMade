
using CheckMade.Common.FpExt.MonadicWrappers;

namespace CheckMade.Telegram.Model;

// ToDo: Add botPrompt (or similar) to capture if this input was a user's reaction to a prompt by the bot
// e.g. clicking on a particular inline keyboard reply button or a custom keyboard button or botCommand
// each botPrompt has a semantic ID, which is also the key to its U.I. string
// should also contain/imply (or separate field?) which botType user is involved with

public record MessageDetails(
    DateTime TelegramDate,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType);