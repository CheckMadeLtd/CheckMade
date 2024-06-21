-- This script is executed in CI environment by the apply_migration.sh script
-- Probably NOT suitable to reuse in local DEV env because of more long-lived DB leading to different id's. 

WITH new_user_daniel AS (
    INSERT INTO users (mobile, first_name, middle_name, last_name, email, status, language_setting)
        VALUES ('+447538521999', '_Daniel', 'IntegrationTest', '_Gorin', 'daniel-integrtest-checkmade@neocortek.net', 0, 0)
        ON CONFLICT (mobile) WHERE status = 0
            DO UPDATE SET status = users.status -- a fake update just so we can return the id
        RETURNING id 
),

-- To test correct handling of absence of optional value
new_user_patrick_without_email AS (
     INSERT INTO users (mobile, first_name, middle_name, last_name, status, language_setting)
         VALUES ('+4999999999', '_Patrick','IntegrationTest', '_Bauer', 0, 0)
         ON CONFLICT (mobile) WHERE status = 0 
             DO UPDATE SET status = users.status
         RETURNING id
),    

new_live_event_venue AS (
    INSERT INTO live_event_venues (name, status) 
        VALUES ('IntegrationTest Venue near Cologne', 0) 
        ON CONFLICT (name)
            DO UPDATE SET status = live_event_venues.status
        RETURNING id
),
    
new_live_event_series AS (
    INSERT INTO live_event_series (name, status) 
       VALUES ('IntegrationTest Parookaville Series', 0)
        ON CONFLICT (name)
            DO UPDATE SET status = live_event_series.status
        RETURNING id
),
    
new_live_event AS (
    INSERT INTO live_events (name, start_date, end_date, venue_id, live_event_series_id, status)
        VALUES ('IntegrationTest Parooka 2024',
                '2024-07-19 10:00:00', '2024-07-22 18:00:00', 
                (SELECT id FROM new_live_event_venue), 
                (SELECT id FROM new_live_event_series), 
                0)
        ON CONFLICT (name)
            DO UPDATE SET status = live_events.status
        RETURNING id
),
    
new_role_for_user_without_email AS (
    INSERT INTO roles (token, role_type, status, user_id, live_event_id)
        VALUES ('RAAAA2', 1003, 0, 
                (SELECT id FROM new_user_patrick_without_email),
                (SELECT id FROM new_live_event))
        ON CONFLICT (token) DO NOTHING
)

INSERT INTO roles (token, role_type, status, user_id, live_event_id) 
    VALUES ('RAAAA1', 1002, 0, (SELECT id FROM new_user_daniel), (SELECT id FROM new_live_event))
    ON CONFLICT (token) DO NOTHING;
