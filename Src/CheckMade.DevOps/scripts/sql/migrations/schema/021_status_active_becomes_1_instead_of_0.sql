-- Changed status int for DbRecordStatus.Active from 0 to 1. 
-- Need to change partial indexes accordingly.

DROP INDEX roles_type_user_live_event_when_active;
CREATE UNIQUE INDEX roles_type_user_live_event_when_active
    ON roles (role_type, user_id, live_event_id)
    WHERE status = 1;

DROP INDEX users_mobile_key_when_active;
CREATE UNIQUE INDEX users_mobile_key_when_active
    ON users (mobile)
    WHERE status = 1;
