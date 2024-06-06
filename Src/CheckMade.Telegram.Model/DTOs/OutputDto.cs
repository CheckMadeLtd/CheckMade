using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto
{
    public Option<TelegramOutputDestination> ExplicitDestination { get; }
    public Option<UiString> Text { get; }
    public Option<IEnumerable<DomainCategory>> DomainCategorySelection { get; }
    public Option<IEnumerable<ControlPrompts>> ControlPromptsSelection { get; }
    public Option<IEnumerable<string>> PredefinedChoices { get; }
    public Option<IEnumerable<OutputAttachmentDetails>> Attachments { get; }
    public Option<Geo> Location { get; }

    private OutputDto(
        Option<TelegramOutputDestination> explicitDestination,
        Option<UiString> text,
        Option<IEnumerable<DomainCategory>> domainCategories,
        Option<IEnumerable<ControlPrompts>> controlPrompts,
        Option<IEnumerable<string>> predefinedChoices, 
        Option<IEnumerable<OutputAttachmentDetails>> attachments, 
        Option<Geo> location)
    {
        ExplicitDestination = explicitDestination;
        Text = text;
        DomainCategorySelection = domainCategories;
        ControlPromptsSelection = controlPrompts;
        PredefinedChoices = predefinedChoices;
        Attachments = attachments;
        Location = location;
    }

    public static OutputDto Create(UiString text) => 
        new(
            Option<TelegramOutputDestination>.None(), 
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());
    
    public static OutputDto Create(TelegramOutputDestination destination, UiString text) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(TelegramOutputDestination destination, IEnumerable<DomainCategory> domainCategories) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(TelegramOutputDestination destination, IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(TelegramOutputDestination destination, IEnumerable<string> predefinedChocies) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.Some(predefinedChocies),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination, UiString text, IEnumerable<DomainCategory> domainCategories) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination, UiString text, IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination,
        UiString text,
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination,
        UiString text,
        IEnumerable<string> predefinedChoices) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.Some(predefinedChoices),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination,
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.None(),
            Option<Geo>.None());

    public static OutputDto Create(
        UiString text,
        IEnumerable<OutputAttachmentDetails> attachmentDetails) =>
        new(
            Option<TelegramOutputDestination>.None(), 
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.Some(attachmentDetails),
            Option<Geo>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination,
        UiString text,
        IEnumerable<OutputAttachmentDetails> attachmentDetails) =>
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None(),
            Option<IEnumerable<OutputAttachmentDetails>>.Some(attachmentDetails),
            Option<Geo>.None());
    
    public static OutputDto Create(
        UiString text,
        Geo location) =>
        new(
            Option<TelegramOutputDestination>.None(), 
            text,
            Option<IEnumerable<DomainCategory>>.None(), 
            Option<IEnumerable<ControlPrompts>>.None(), 
            Option<IEnumerable<string>>.None(), 
            Option<IEnumerable<OutputAttachmentDetails>>.None(), 
            location);
    
    public static OutputDto Create(
        TelegramOutputDestination destination,
        UiString text,
        Geo location) =>
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(), 
            Option<IEnumerable<ControlPrompts>>.None(), 
            Option<IEnumerable<string>>.None(), 
            Option<IEnumerable<OutputAttachmentDetails>>.None(), 
            location);
}
