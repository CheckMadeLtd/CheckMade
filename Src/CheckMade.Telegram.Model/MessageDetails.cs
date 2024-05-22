
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Model;

// ToDo: Add SentToBotType

public record MessageDetails(
    DateTime TelegramDate,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType,
    Option<SubmissionsBotCommands> SubmissionsBotCommand);