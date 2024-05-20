// using System.Collections.Immutable;
// using System.Globalization;
// using CheckMade.Common.FpExt.MonadicWrappers;
// using CheckMade.Common.Persistence;
// using CheckMade.Common.Utils;
// using CheckMade.DevOps.DataMigration.Repositories;
//
// namespace CheckMade.DevOps.DataMigration.Migrators;
//
// internal class Mig0002(MessagesMigrationRepository migRepo) : DataMigratorBase(migRepo)
// {
//     protected override Attempt<IEnumerable<UpdateDetails>> SafelyGenerateMigrationUpdatesAsync(
//         IEnumerable<MessageOldFormatDetailsPair> allHistoricMessageDetailPairs)
//     {
//         var updateDetailsBuilder = ImmutableArray.CreateBuilder<UpdateDetails>();
//
//         try
//         {
//             foreach (var pair in allHistoricMessageDetailPairs)
//             {
//                 DateTime.TryParseExact(
//                     pair.OldFormatDetailsJson.Value<string>("TelegramDate"), 
//                     "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, 
//                     out var telegramDate);
//                 
//                 var updateDetailsForCurrentPair = new UpdateDetails(
//                     pair.ModelMessage.UserId, telegramDate,
//                     new Dictionary<string, object>());
//
//                 
//                 
//                 if (updateDetailsForCurrentPair.NewValueByColumn.Count > 0)
//                     updateDetailsBuilder.Add(updateDetailsForCurrentPair);
//             }
//         }
//         catch (Exception ex)
//         {
//             return Attempt<IEnumerable<UpdateDetails>>
//                 .Fail(new DataMigrationException(
//                     $"Exception while generating updates for data migration: {ex.Message}", ex));
//         }
//
//         return Attempt<IEnumerable<UpdateDetails>>
//             .Succeed(updateDetailsBuilder.ToImmutable());
//     }
// }