using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class RoleRepository(IDbExecutionHelper dbHelper) : BaseRepository(dbHelper), IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        const string rawQuery = "SELECT " +
                                "usr.mobile AS user_mobile, " +
                                "usr.first_name AS user_first_name, " +
                                "usr.middle_name AS user_middle_name, " +
                                "usr.last_name AS user_last_name, " +
                                "usr.email AS user_email, " +
                                "usr.language_setting AS user_language, " +
                                "usr.status AS user_status, " +
                                "ven.name AS venue_name, " +
                                "ven.status AS venue_status, " +
                                "lve.name AS live_event_name, " +
                                "lve.start_date AS live_event_start_date, " +
                                "lve.end_date AS live_event_end_date, " +
                                "lve.status AS live_event_status, " +
                                "r.token AS role_token, " +
                                "r.role_type AS role_type, " +
                                "r.status AS role_status " +
                                "FROM roles r " +
                                "INNER JOIN users usr on r.user_id = usr.id " +
                                "INNER JOIN live_events lve on r.live_event_id = lve.id " +
                                "INNER JOIN live_event_venues ven on lve.venue_id = ven.id";
        
        var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

        return await ExecuteReaderAsync(command, reader =>
        {
            var user = new User(
                new MobileNumber(reader.GetString(reader.GetOrdinal("user_mobile"))),
                reader.GetString(reader.GetOrdinal("user_first_name")),
                GetOption<string>(reader, reader.GetOrdinal("user_middle_name")),
                reader.GetString(reader.GetOrdinal("user_last_name")),
                GetOption<EmailAddress>(reader, reader.GetOrdinal("user_email")),
                (LanguageCode)reader.GetInt16(reader.GetOrdinal("user_language")),
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("user_status")));

            var venue = new LiveEventVenue(
                reader.GetString(reader.GetOrdinal("venue_name")),
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("venue_status")));
            
            var liveEvent = new LiveEvent(
                    reader.GetString(reader.GetOrdinal("live_event_name")),
                    reader.GetDateTime(reader.GetOrdinal("live_event_start_date")),
                    reader.GetDateTime(reader.GetOrdinal("live_event_end_date")),
                    // We leave this list empty to avoid unnecessary circular references in our object graph
                    new List<Role>(),
                    venue,
                    (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("live_event_status")));
            
            return new Role(
                reader.GetString(reader.GetOrdinal("role_token")),
                (RoleType)reader.GetInt16(reader.GetOrdinal("role_type")),
                user,
                liveEvent,
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status")));
        });
    }
}