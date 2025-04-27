using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;
using CheckMade.Common.Model.ChatBot;

namespace CheckMade.Common.Model;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    TlgAttachmentType AttachmentType,
    Option<string> Caption);