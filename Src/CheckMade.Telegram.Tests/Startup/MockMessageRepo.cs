using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Moq;

namespace CheckMade.Telegram.Tests.Startup;

public class MockMessageRepo(IMock<IMessageRepo> mockMessageRepo) : IMessageRepo
{
    public void Add(InputTextMessage inputMessage)
    {
        mockMessageRepo.Object.Add(inputMessage);
    }

    public IEnumerable<InputTextMessage> GetAll(long userId)
    {
        return mockMessageRepo.Object.GetAll(userId);
    }
}