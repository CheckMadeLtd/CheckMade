namespace CheckMade.Telegram.Model;

// Explicit numbers to avoid 'Submissions' implicilty being the default (= 0).
// There shouldn't be a 'default' e.g. to avoid Json Serializer to assigning it implicitly in some cases.

public enum BotType
{
    Submissions = 1,
    Communications = 2,
    Notifications = 3
}