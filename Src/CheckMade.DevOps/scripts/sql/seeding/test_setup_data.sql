-- This scripts fills db with data required by integration tests
-- Note: it is automatically executed in CI environment by the apply_migration.sh script!

-- Important: the data here needs to be kept in-sync with CheckMade.Tests.Utils.TestData !!

WITH ci_user_daniel_en AS (
    INSERT INTO users (mobile, first_name, middle_name, last_name, email, status, language_setting)
        VALUES ('+447777111999', '_Daniel', 'Test English', '_Gorin', 'daniel-test-checkmade@neocortek.net', 1, 0)
        ON CONFLICT (mobile) WHERE status = 1
            DO UPDATE SET status = users.status -- a fake update just so we can return the id
        RETURNING id 
),

-- To test correct handling of absence of optional value
ci_user_lukas_de_without_email AS (
     INSERT INTO users (mobile, first_name, middle_name, last_name, status, language_setting)
         VALUES ('+49111199999', '_Lukas','Test German', '_Gorin', 1, 1)
         ON CONFLICT (mobile) WHERE status = 1 
             DO UPDATE SET status = users.status
         RETURNING id
),    

ci_live_event_venue AS (
    INSERT INTO live_event_venues (name, status) 
        VALUES ('Venue1 near Cologne', 1) 
        ON CONFLICT (name)
            DO UPDATE SET status = live_event_venues.status
        RETURNING id
),
    
ci_live_event_series AS (
    INSERT INTO live_event_series (name, status) 
       VALUES ('LiveEvent Series X', 1)
        ON CONFLICT (name)
            DO UPDATE SET status = live_event_series.status
        RETURNING id
),
    
ci_live_event_X2024 AS (
    INSERT INTO live_events (name, start_date, end_date, venue_id, live_event_series_id, status)
        VALUES ('LiveEvent X 2024',
                '2024-07-19 10:00:00', '2024-07-22 18:00:00', 
                (SELECT id FROM ci_live_event_venue), 
                (SELECT id FROM ci_live_event_series), 
                1)
        ON CONFLICT (name)
            DO UPDATE SET status = live_events.status
        RETURNING id
),

ci_live_event_X2025 AS (
    INSERT INTO live_events (name, start_date, end_date, venue_id, live_event_series_id, status)
        VALUES ('LiveEvent X 2025',
                '2025-07-18 10:00:00', '2025-07-21 18:00:00',
                (SELECT id FROM ci_live_event_venue),
                (SELECT id FROM ci_live_event_series),
                1)
        ON CONFLICT (name)
            DO UPDATE SET status = live_events.status
        RETURNING id
),

ci_role_for_lukas_de_without_email AS (
    INSERT INTO roles (token, role_type, status, user_id, live_event_id)
        VALUES ('R7UIP8', 1002, 1, 
                (SELECT id FROM ci_user_lukas_de_without_email),
                (SELECT id FROM ci_live_event_X2024))
        ON CONFLICT (token) DO NOTHING
),

ci_role_for_daniel_en_x2025 AS (
    INSERT INTO roles (token, role_type, status, user_id, live_event_id)
        VALUES ('R9AAB5', 1002, 1,
                (SELECT id FROM ci_user_daniel_en),
                (SELECT id FROM ci_live_event_X2025))
        ON CONFLICT (token) DO NOTHING
)

INSERT INTO roles (token, role_type, status, user_id, live_event_id) 
    VALUES ('RVB70T', 1001, 1,
            (SELECT id FROM ci_user_daniel_en),
            (SELECT id FROM ci_live_event_X2024))
    ON CONFLICT (token) DO NOTHING;
