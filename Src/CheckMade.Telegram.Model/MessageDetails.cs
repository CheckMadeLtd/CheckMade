
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    BotType RecipientBotType,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType,
    Option<SubmissionsBotCommands> SubmissionsBotCommand);