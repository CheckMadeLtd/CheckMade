using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversion;
using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services.UpdateHandling;

internal static class OutputSender
{
        internal static async Task<Unit> SendOutputsAsync(
            IReadOnlyList<OutputDto> outputs,
            IDictionary<BotType, IBotClientWrapper> botClientByBotType,
            BotType updateReceivingBotType,
            ChatId updateReceivingChatId,
            IDictionary<TelegramOutputDestination, TelegramChatId> chatIdByOutputDestination,
            IUiTranslator uiTranslator,
            IOutputToReplyMarkupConverter converter,
            IBlobLoader blobLoader)
    {
        Func<IReadOnlyCollection<OutputDto>, Task> sendOutputsInSeriesAndOriginalOrder 
            = async outputsPerDestination =>
        {
            foreach (var output in outputsPerDestination)
            {
                var destinationBotClient = output.ExplicitDestination.Match(
                    destination => botClientByBotType[destination.DestinationBotType],
                    () => botClientByBotType[updateReceivingBotType]); // e.g. for a virgin, pre-login update

                var destinationChatId = output.ExplicitDestination.Match(
                    destination => chatIdByOutputDestination[destination].Id,
                    () => updateReceivingChatId); // e.g. for a virgin, pre-login update
                    
                switch (output)
                {
                    case { Text.IsSome: true, Attachments.IsSome: false, Location.IsSome: false }:
                        await InvokeSendTextMessageAsync(output.Text.GetValueOrThrow());
                        break;

                    case { Attachments.IsSome: true }:
                        if (output.Text.IsSome)
                            await InvokeSendTextMessageAsync(output.Text.GetValueOrThrow());
                        foreach (var attachment in output.Attachments.GetValueOrThrow())
                            await InvokeSendAttachmentAsync(attachment);
                        break;

                    case { Location.IsSome: true }:
                        await InvokeSendLocationAsync(output.Location.GetValueOrThrow());
                        break;
                }

                continue;

                async Task InvokeSendTextMessageAsync(UiString outputText)
                {
                    await destinationBotClient
                        .SendTextMessageAsync(
                            destinationChatId,
                            uiTranslator.Translate(Ui("Please choose:")),
                            uiTranslator.Translate(outputText),
                            converter.GetReplyMarkup(output));
                }

                async Task InvokeSendAttachmentAsync(OutputAttachmentDetails details)
                {
                    var (blobData, fileName) =
                        await blobLoader.DownloadBlobAsync(details.AttachmentUri);
                    var fileStream = new InputFileStream(blobData, fileName);
                    
                    var caption = details.Caption.Match(
                        value => Option<string>.Some(uiTranslator.Translate(value)),
                        Option<string>.None);

                    var attachmentSendOutParams = new AttachmentSendOutParameters(
                        destinationChatId,
                        fileStream,
                        caption,
                        converter.GetReplyMarkup(output)
                    );

                    switch (details.AttachmentType)
                    {
                        case AttachmentType.Document:
                            await destinationBotClient.SendDocumentAsync(attachmentSendOutParams);
                            break;

                        case AttachmentType.Photo:
                            await destinationBotClient.SendPhotoAsync(attachmentSendOutParams);
                            break;

                        case AttachmentType.Voice:
                            await destinationBotClient.SendVoiceAsync(attachmentSendOutParams);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(details.AttachmentType));
                    }
                }

                async Task InvokeSendLocationAsync(Geo location)
                {
                    await destinationBotClient
                        .SendLocationAsync(
                            destinationChatId,
                            location,
                            converter.GetReplyMarkup(output));
                }
            }
        };

        var outputGroups = outputs.GroupBy(o => o.ExplicitDestination);

        var parallelTasks = outputGroups
            .Select(outputsPerDestinationGroup => 
                sendOutputsInSeriesAndOriginalOrder.Invoke(outputsPerDestinationGroup.ToList().AsReadOnly()));
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        await Task.WhenAll(parallelTasks);
        
        return Unit.Value;
    }
}