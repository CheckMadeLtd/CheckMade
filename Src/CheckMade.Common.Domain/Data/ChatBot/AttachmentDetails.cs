using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.ChatBot;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    AttachmentType AttachmentType,
    Option<string> Caption);