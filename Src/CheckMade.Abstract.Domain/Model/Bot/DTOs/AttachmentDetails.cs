using CheckMade.Abstract.Domain.Model.Bot.Categories;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs;

public sealed record AttachmentDetails(
    Uri AttachmentUri,
    AttachmentType AttachmentType,
    Option<string> Caption);