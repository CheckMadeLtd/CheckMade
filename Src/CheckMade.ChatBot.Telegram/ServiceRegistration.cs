using CheckMade.ChatBot.Telegram.BotClient;
using CheckMade.ChatBot.Telegram.Conversion;
using CheckMade.ChatBot.Telegram.Function;
using CheckMade.ChatBot.Telegram.UpdateHandling;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Telegram;

public static class ServiceRegistration
{
    public static void RegisterChatBotTelegramFunctionServices(this IServiceCollection services)
    {
        services.AddScoped<IBotFunction, TelegramBotFunction>();
    }
    
    public static void RegisterChatBotTelegramUpdateHandlingServices(this IServiceCollection services)
    {
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<IBotUpdateSwitch, BotUpdateSwitch>();
        services.AddSingleton<ILastOutputMessageIdCache, LastOutputTelegramMessageIdCache>();
    }
    
    public static void RegisterChatBotTelegramConversionServices(this IServiceCollection services)
    {
        services.AddScoped<IToModelConverterFactory, ToModelConverterFactory>();
        services.AddScoped<IOutputToReplyMarkupConverterFactory, OutputToReplyMarkupConverterFactory>();
    }
    
    public static void RegisterChatBotTelegramBotClientServices(this IServiceCollection services)
    {
        services.AddSingleton<IBotClientFactory, BotClientFactory>();

        var interactionModes = Enum.GetNames(typeof(InteractionMode));
        foreach (var mode in interactionModes)
        {
            services.AddHttpClient($"CheckMade{mode}Bot");            
        }    
    }
}