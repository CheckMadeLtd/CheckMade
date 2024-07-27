using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Utils.Generic;
using static CheckMade.Tests.Utils.TestOriginatorRoleSetting;

namespace CheckMade.Tests.Utils;

internal interface ITlgInputGenerator
{
    TlgInput GetValidTlgInputTextMessage(
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        string text = "Hello World", DateTimeOffset? dateTime = null,
        TestOriginatorRoleSetting roleSetting = Default,
        Role? roleSpecified = null,
        ResultantWorkflowInfo? resultantWorkflowInfo = null);
    
    TlgInput GetValidTlgInputTextMessageWithAttachment(
        TlgAttachmentType type,
        TestOriginatorRoleSetting roleSetting = Default);
    
    TlgInput GetValidTlgInputLocationMessage(
        Geo location, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        DateTimeOffset? dateTime = null, 
        TestOriginatorRoleSetting roleSetting = Default,
        ResultantWorkflowInfo? resultantWorkflowInfo = null);
    
    TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        int messageId = 1,
        TestOriginatorRoleSetting roleSetting = Default,
        Role? roleSpecified = null,
        ResultantWorkflowInfo? resultantWorkflowInfo = null);
    
    TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        DateTimeOffset? dateTime = null, int messageId = 1,
        TestOriginatorRoleSetting roleSetting = Default,
        ResultantWorkflowInfo? resultantWorkflowInfo = null);
    
    TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        DateTimeOffset? dateTime = null,
        TestOriginatorRoleSetting roleSetting = Default,
        ResultantWorkflowInfo? resultantWorkflowInfo = null);
}

internal sealed class TlgInputGenerator(Randomizer randomizer) : ITlgInputGenerator
{
    public Randomizer Randomizer { get; } = randomizer;
    
    public TlgInput GetValidTlgInputTextMessage(
        long userId, long chatId, string text, DateTimeOffset? dateTime,
        TestOriginatorRoleSetting roleSetting,
        Role? roleSpecified,
        ResultantWorkflowInfo? resultantWorkflowInfo)
    {
        var (originatorRole, liveEvent) = 
            GetOriginatorRoleAndLiveEventFromArgs(roleSetting, roleSpecified);
        
        return new TlgInput(
            dateTime ?? DateTimeOffset.UtcNow, 
            1, 
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.TextMessage,
            originatorRole, 
            liveEvent, 
            resultantWorkflowInfo ?? Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            CreateFromRelevantDetails(
                text));
    }

    public TlgInput GetValidTlgInputTextMessageWithAttachment(
        TlgAttachmentType type, 
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            DateTimeOffset.UtcNow,
            1,
            new TlgAgent(Default_UserAndChatId_PrivateBotChat,
                Default_UserAndChatId_PrivateBotChat,
                Operations),
            TlgInputType.AttachmentMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            CreateFromRelevantDetails(
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                new Uri("https://www.gorin.de/fakeTelegramUri1.html"),
                new Uri("https://www.gorin.de/fakeInternalUri1.html"),
                type));
    }

    public TlgInput GetValidTlgInputLocationMessage(
        Geo location,
        long userId, long chatId, DateTimeOffset? dateTime,
        TestOriginatorRoleSetting roleSetting,
        ResultantWorkflowInfo? resultantWorkflowInfo)
    {
        return new TlgInput(
            dateTime ?? DateTimeOffset.UtcNow, 
            1,
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.Location,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            resultantWorkflowInfo ?? Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            CreateFromRelevantDetails(
                geoCoordinates: location));
    }

    public TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode,
        long userId, long chatId, int messageId,
        TestOriginatorRoleSetting roleSetting,
        Role? roleSpecified,
        ResultantWorkflowInfo? resultantWorkflowInfo)
    {
        var (originatorRole, liveEvent) = 
            GetOriginatorRoleAndLiveEventFromArgs(roleSetting, roleSpecified);
        
        return new TlgInput(
            DateTimeOffset.UtcNow,
            messageId,
            new TlgAgent(userId, chatId, interactionMode),
            TlgInputType.CommandMessage,
            originatorRole, 
            liveEvent, 
            resultantWorkflowInfo ?? Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            CreateFromRelevantDetails(
                botCommandEnumCode: botCommandEnumCode));
    }

    public TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm,
        long userId, long chatId, DateTimeOffset? dateTime, int messageId,
        TestOriginatorRoleSetting roleSetting,
        ResultantWorkflowInfo? resultantWorkflowInfo)
    {
        return new TlgInput(
            dateTime ?? DateTimeOffset.UtcNow,
            messageId,
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            resultantWorkflowInfo ?? Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            CreateFromRelevantDetails(
                domainTerm: domainTerm));
    }

    public TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts,
        long userId, long chatId, DateTimeOffset? dateTime,
        TestOriginatorRoleSetting roleSetting,
        ResultantWorkflowInfo? resultantWorkflowInfo)
    {
        return new TlgInput(
            dateTime ?? DateTimeOffset.UtcNow,
            1,
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            resultantWorkflowInfo ?? Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            CreateFromRelevantDetails(
                controlPromptEnumCode: (long)prompts));
    }

    internal static TlgInputDetails CreateFromRelevantDetails(
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
        GetOriginatorRoleAndLiveEventFromArgs(
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

        return (originatorRole, liveEvent);
    }
    
    private static (Option<IRoleInfo> originatorRole, Option<ILiveEventInfo> liveEvent)
        GetInputContextInfo(TestOriginatorRoleSetting roleSetting)
    {
        return roleSetting switch
        {
            None => 
                (Option<IRoleInfo>.None(),
                    Option<ILiveEventInfo>.None()),
            
            Default =>
                (SaniCleanAdmin_DanielEn_X2024,
                    Option<ILiveEventInfo>.Some(SaniCleanAdmin_DanielEn_X2024.AtLiveEvent)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(roleSetting))
        };
    }
}

internal enum TestOriginatorRoleSetting
{
    Default,
    None
}
