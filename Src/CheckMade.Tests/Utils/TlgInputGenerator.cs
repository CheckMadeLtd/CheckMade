using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Utils.Generic;
using static CheckMade.Tests.Utils.TestOriginatorRoleSetting;

namespace CheckMade.Tests.Utils;

internal interface ITlgInputGenerator
{
    TlgInput GetValidTlgInputTextMessage(
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        string text = "Hello World", DateTime? dateTime = null,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault,
        Role? roleSpecified = null);
    
    TlgInput GetValidTlgInputTextMessageWithAttachment(
        TlgAttachmentType type,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputLocationMessage(
        double latitudeRaw, double longitudeRaw, Option<float> uncertaintyRadius, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        int messageId = 1,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        DateTime? dateTime = null, int messageId = 1,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        DateTime? dateTime = null,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
}

internal class TlgInputGenerator(Randomizer randomizer) : ITlgInputGenerator
{
    public Randomizer Randomizer { get; } = randomizer;
    
    public TlgInput GetValidTlgInputTextMessage(
        long userId, long chatId, string text, DateTime? dateTime,
        TestOriginatorRoleSetting roleSetting,
        Role? roleSpecified)
    {
        Option<IRoleInfo> originatorRole;
        Option<ILiveEventInfo> liveEvent;

        if (roleSpecified != null)
        {
            originatorRole = roleSpecified;
            liveEvent = Option<ILiveEventInfo>.Some(roleSpecified.AtLiveEvent);
        }
        else
        {
            originatorRole = GetInputContextInfo(roleSetting).originatorRole;
            liveEvent = GetInputContextInfo(roleSetting).liveEvent;
        }
            
        
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.TextMessage,
            originatorRole, 
            liveEvent, 
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow, 
                1, 
                text));
    }

    public TlgInput GetValidTlgInputTextMessageWithAttachment(
        TlgAttachmentType type, 
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(Default_UserAndChatId_PrivateBotChat,
                Default_UserAndChatId_PrivateBotChat,
                Operations),
            TlgInputType.AttachmentMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                1,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                new Uri("https://www.gorin.de/fakeTelegramUri1.html"),
                new Uri("https://www.gorin.de/fakeInternalUri1.html"),
                type));
    }

    public TlgInput GetValidTlgInputLocationMessage(
        double latitudeRaw, double longitudeRaw, Option<float> uncertaintyRadius,
        long userId, long chatId,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.Location,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                DateTime.UtcNow, 
                1,
                geoCoordinates: new Geo(latitudeRaw, longitudeRaw, uncertaintyRadius)));
    }

    public TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode,
        long userId, long chatId, int messageId,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, interactionMode),
            TlgInputType.CommandMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                messageId,
                botCommandEnumCode: botCommandEnumCode));
    }

    public TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm,
        long userId, long chatId, DateTime? dateTime, int messageId,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                messageId,
                domainTerm: domainTerm));
    }

    public TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts,
        long userId, long chatId, DateTime? dateTime,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                1,
                controlPromptEnumCode: (long)prompts));
    }

    internal static TlgInputDetails CreateFromRelevantDetails(
        DateTime tlgDate,
        int tlgMessageId,
        string? text = null,
        Uri? attachmentTlgUri = null,
        Uri? attachmentInternalUri = null,
        TlgAttachmentType? attachmentType = null,
        Geo? geoCoordinates = null,
        int? botCommandEnumCode = null,
        DomainTerm? domainTerm = null,
        long? controlPromptEnumCode = null)
    {
        return new TlgInputDetails(
            tlgDate, 
            tlgMessageId,
            text ?? Option<string>.None(),
            attachmentTlgUri ?? Option<Uri>.None(),
            attachmentInternalUri ?? Option<Uri>.None(), 
            attachmentType ?? Option<TlgAttachmentType>.None(),
            geoCoordinates ?? Option<Geo>.None(),
            botCommandEnumCode ?? Option<int>.None(),
            domainTerm ?? Option<DomainTerm>.None(),
            controlPromptEnumCode ?? Option<long>.None());
    }
    
    private static (Option<IRoleInfo> originatorRole, Option<ILiveEventInfo> liveEvent)
        GetInputContextInfo(TestOriginatorRoleSetting roleSetting)
    {
        return roleSetting switch
        {
            None => 
                (Option<IRoleInfo>.None(),
                    Option<ILiveEventInfo>.None()),
            
            UnitTestDefault =>
                (SOpsAdmin_DanielEn_X2024,
                    Option<ILiveEventInfo>.Some(SOpsAdmin_DanielEn_X2024.AtLiveEvent)),
            
            IntegrationTestDefault =>
                (IntegrationTests_SOpsInspector_DanielEn_X2024, 
                    Option<ILiveEventInfo>.Some(IntegrationTests_SOpsInspector_DanielEn_X2024.AtLiveEvent)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(roleSetting))
        };
    }
}

internal enum TestOriginatorRoleSetting
{
    UnitTestDefault,
    IntegrationTestDefault,
    None
}
