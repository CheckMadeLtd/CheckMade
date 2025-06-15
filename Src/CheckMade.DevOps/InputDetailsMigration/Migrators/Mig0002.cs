// namespace CheckMade.DevOps.DetailsMigration.InputMessages.Migrators;

// internal class Mig0002(MigrationRepository migRepo) : MigratorBase(migRepo)
// {
//     protected override Attempt<IEnumerable<DetailsUpdate>> GenerateMigrationUpdatesAsync(
//         IEnumerable<OldFormatDetailsPair> allHistoricMessageDetailPairs)
    // {
        /*
         * Postprocessing after SQL Mig 006
         * - Moving RecipientBotType out of Details, into the new dedicated field in tlgr_messages
         * Decided to do it manually / delete old messages to save time. Because in this case, we need to not just
         * change/update Details (which the current code base supports) but also write into the tlgr_messages table,
         * which is currently not supported by the overall DetailsMigration logic.
         *
         * Maybe I need to bring back the ability to also update non-Details fields? Apparently! 
         */
        
//         
//     }
// }