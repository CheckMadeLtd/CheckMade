namespace CheckMade.Telegram.Model;

public record TelegramUserId(long Id)
{
    public static implicit operator long(TelegramUserId userId) => userId.Id;
    public static implicit operator TelegramUserId(long id) => new TelegramUserId(id);
}