namespace CheckMade.Abstract.Domain.Data.ChatBot.Input;

public enum InputType
{
    CommandMessage = 10,
    TextMessage = 11,
    AttachmentMessage = 12,
    CallbackQuery = 20,
    Location = 30
}