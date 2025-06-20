using CheckMade.Core.Model.Bot.Categories;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Bot.DTOs;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    AttachmentType AttachmentType,
    Option<string> Caption);