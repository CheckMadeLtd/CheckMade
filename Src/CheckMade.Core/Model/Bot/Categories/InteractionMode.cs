namespace CheckMade.Core.Model.Bot.Categories;

// Avoids 'Operations' implicilty being the default (= 0).
// There shouldn't be a 'default' e.g. to avoid Json Serializer to assigning it implicitly in some cases.

public enum InteractionMode
{
    Operations = 1,
    Communications,
    Notifications
}