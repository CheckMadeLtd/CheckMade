namespace CheckMade.Common.Model.Tlg.Updates;

// Explicit numbers to avoid 'Operations' implicilty being the default (= 0).
// There shouldn't be a 'default' e.g. to avoid Json Serializer to assigning it implicitly in some cases.

public enum TlgBotType
{
    Operations = 1,
    Communications = 2,
    Notifications = 3
}