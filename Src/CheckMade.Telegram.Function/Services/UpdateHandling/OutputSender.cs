using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services.UpdateHandling;

internal static class OutputSender
{
        internal static async Task<Unit> SendOutputsOrThrowAsync(
            IReadOnlyList<OutputDto> outputs,
            IDictionary<BotType, IBotClientWrapper> botClientByBotType,
            BotType updateReceivingBotType,
            ChatId updateReceivingChatId,
            IDictionary<TelegramOutputDestination, TelegramChatId> chatIdByOutputDestination,
            IUiTranslator uiTranslator,
            IOutputToReplyMarkupConverter converter,
            IBlobLoader blobLoader)
    {
        Func<IReadOnlyCollection<OutputDto>, Task> sendOutputsInSeries = async outputsPerDestination =>
        {
            foreach (var output in outputsPerDestination)
            {
                var destinationBotClient = output.ExplicitDestination.IsSome
                    ? botClientByBotType[output.ExplicitDestination.Value!.DestinationBotType]
                    : botClientByBotType[updateReceivingBotType]; // e.g. for a virgin, pre-login update

                var destinationChatId = output.ExplicitDestination.IsSome
                    ? chatIdByOutputDestination[output.ExplicitDestination.Value!].Id
                    : updateReceivingChatId; // e.g. for a virgin, pre-login update

                switch (output)
                {
                    case { Attachments.IsSome: false, Location.IsSome: false }:
                        await InvokeSendTextMessageOrThrowAsync();
                        break;

                    case { Attachments.IsSome: true }:
                        foreach (var attachment in output.Attachments.Value!)
                            await InvokeSendAttachmentOrThrowAsync(attachment);
                        break;

                    case { Location.IsSome: true }:
                        await InvokeSendLocationOrThrowAsync();
                        break;
                }

                continue;

                async Task InvokeSendTextMessageOrThrowAsync()
                {
                    await destinationBotClient
                        .SendTextMessageOrThrowAsync(
                            destinationChatId,
                            uiTranslator.Translate(Ui("Please choose:")),
                            uiTranslator.Translate(output.Text.GetValueOrDefault(Ui())),
                            converter.GetReplyMarkup(output));
                }

                async Task InvokeSendAttachmentOrThrowAsync(OutputAttachmentDetails details)
                {
                    var (blobData, fileName) =
                        await blobLoader.DownloadBlobOrThrowAsync(details.AttachmentUri);
                    var fileStream = new InputFileStream(blobData, fileName);

                    var attachmentSendOutParams = new AttachmentSendOutParameters(
                        DestinationChatId: destinationChatId,
                        FileStream: fileStream,
                        Caption: Option<string>.Some(uiTranslator.Translate(output.Text.GetValueOrDefault(Ui()))),
                        ReplyMarkup: converter.GetReplyMarkup(output)
                    );

                    switch (details.AttachmentType)
                    {
                        case AttachmentType.Document:
                            await destinationBotClient.SendDocumentOrThrowAsync(attachmentSendOutParams);
                            break;

                        case AttachmentType.Photo:
                            await destinationBotClient.SendPhotoOrThrowAsync(attachmentSendOutParams);
                            break;

                        case AttachmentType.Voice:
                            await destinationBotClient.SendVoiceOrThrowAsync(attachmentSendOutParams);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(details.AttachmentType));
                    }
                }

                async Task InvokeSendLocationOrThrowAsync()
                {
                    await destinationBotClient
                        .SendLocationOrThrowAsync(
                            destinationChatId,
                            output.Location.Value!,
                            converter.GetReplyMarkup(output));
                }
            }
        };

        var outputGroups = outputs.GroupBy(o => o.ExplicitDestination);

        var parallelTasks = outputGroups
            .Select(outputsPerDestinationGroup => 
                sendOutputsInSeries.Invoke(outputsPerDestinationGroup.ToList().AsReadOnly()));
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        await Task.WhenAll(parallelTasks);
        
        return Unit.Value;
    }
}