using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Abstract.Domain.Data.Core.GIS;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Tests.Utils.TestOriginatorRoleSetting;

namespace CheckMade.Tests.Utils;

internal interface IInputGenerator
{
    Input GetValidInputTextMessage(
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        string text = "Hello World", 
        DateTimeOffset? dateTime = null,
        TestOriginatorRoleSetting roleSetting = Default,
        Role? roleSpecified = null,
        ResultantWorkflowState? resultantWorkflowState = null);
    
    Input GetValidInputTextMessageWithAttachment(
        AttachmentType type,
        TestOriginatorRoleSetting roleSetting = Default);
    
    Input GetValidInputLocationMessage(
        Geo location, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        DateTimeOffset? dateTime = null, 
        TestOriginatorRoleSetting roleSetting = Default,
        ResultantWorkflowState? resultantWorkflowState = null);
    
    Input GetValidInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        int messageId = 1,
        TestOriginatorRoleSetting roleSetting = Default,
        Role? roleSpecified = null,
        ResultantWorkflowState? resultantWorkflowState = null);
    
    Input GetValidInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        DateTimeOffset? dateTime = null, int messageId = 1,
        TestOriginatorRoleSetting roleSetting = Default,
        ResultantWorkflowState? resultantWorkflowState = null);
    
    Input GetValidInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, 
        string text = "Default fake prompt",
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat,
        DateTimeOffset? dateTime = null,
        TestOriginatorRoleSetting roleSetting = Default,
        ResultantWorkflowState? resultantWorkflowState = null);
}

internal sealed class InputGenerator(Randomizer randomizer) : IInputGenerator
{
    public Randomizer Randomizer { get; } = randomizer;
    
    public Input GetValidInputTextMessage(
        long userId, long chatId, string text, DateTimeOffset? dateTime,
        TestOriginatorRoleSetting roleSetting,
        Role? roleSpecified,
        ResultantWorkflowState? resultantWorkflowState)
    {
        var (originatorRole, liveEvent) = 
            GetOriginatorRoleAndLiveEventFromArgs(roleSetting, roleSpecified);
        
        return new Input(
            dateTime ?? DateTimeOffset.UtcNow, 
            1, 
            new Agent(userId, chatId, Operations),
            InputType.TextMessage,
            originatorRole, 
            liveEvent, 
            resultantWorkflowState ?? Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            CreateFromRelevantDetails(
                text));
    }

    public Input GetValidInputTextMessageWithAttachment(
        AttachmentType type, 
        TestOriginatorRoleSetting roleSetting)
    {
        return new Input(
            DateTimeOffset.UtcNow,
            1,
            new Agent(Default_UserAndChatId_PrivateBotChat,
                Default_UserAndChatId_PrivateBotChat,
                Operations),
            InputType.AttachmentMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            CreateFromRelevantDetails(
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                new Uri("https://www.gorin.de/fakeInternalUri1.html"),
                type));
    }

    public Input GetValidInputLocationMessage(
        Geo location,
        long userId, long chatId, DateTimeOffset? dateTime,
        TestOriginatorRoleSetting roleSetting,
        ResultantWorkflowState? resultantWorkflowState)
    {
        return new Input(
            dateTime ?? DateTimeOffset.UtcNow, 
            1,
            new Agent(userId, chatId, Operations),
            InputType.Location,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            resultantWorkflowState ?? Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            CreateFromRelevantDetails(
                geoCoordinates: location));
    }

    public Input GetValidInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode,
        long userId, long chatId, int messageId,
        TestOriginatorRoleSetting roleSetting,
        Role? roleSpecified,
        ResultantWorkflowState? resultantWorkflowState)
    {
        var (originatorRole, liveEvent) = 
            GetOriginatorRoleAndLiveEventFromArgs(roleSetting, roleSpecified);
        
        return new Input(
            DateTimeOffset.UtcNow,
            messageId,
            new Agent(userId, chatId, interactionMode),
            InputType.CommandMessage,
            originatorRole, 
            liveEvent, 
            resultantWorkflowState ?? Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            CreateFromRelevantDetails(
                botCommandEnumCode: botCommandEnumCode));
    }

    public Input GetValidInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm,
        long userId, long chatId, DateTimeOffset? dateTime, int messageId,
        TestOriginatorRoleSetting roleSetting,
        ResultantWorkflowState? resultantWorkflowState)
    {
        return new Input(
            dateTime ?? DateTimeOffset.UtcNow,
            messageId,
            new Agent(userId, chatId, Operations),
            InputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            resultantWorkflowState ?? Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            CreateFromRelevantDetails(
                domainTerm: domainTerm));
    }

    public Input GetValidInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, string text,
        long userId, long chatId, DateTimeOffset? dateTime,
        TestOriginatorRoleSetting roleSetting,
        ResultantWorkflowState? resultantWorkflowState)
    {
        return new Input(
            dateTime ?? DateTimeOffset.UtcNow,
            1,
            new Agent(userId, chatId, Operations),
            InputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            resultantWorkflowState ?? Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            CreateFromRelevantDetails(
                text: text,
                controlPromptEnumCode: (long)prompts));
    }

    internal static InputDetails CreateFromRelevantDetails(
        string? text = null,
        Uri? attachmentInternalUri = null,
        AttachmentType? attachmentType = null,
        Geo? geoCoordinates = null,
        int? botCommandEnumCode = null,
        DomainTerm? domainTerm = null,
        long? controlPromptEnumCode = null)
    {
        return new InputDetails(
            text ?? Option<string>.None(),
            attachmentInternalUri ?? Option<Uri>.None(), 
            attachmentType ?? Option<AttachmentType>.None(),
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
                (SanitaryAdmin_DanielEn_X2024,
                    Option<ILiveEventInfo>.Some(SanitaryAdmin_DanielEn_X2024.AtLiveEvent)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(roleSetting))
        };
    }
}

internal enum TestOriginatorRoleSetting
{
    Default,
    None
}
