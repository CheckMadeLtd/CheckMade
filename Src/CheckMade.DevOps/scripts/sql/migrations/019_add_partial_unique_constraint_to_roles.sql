-- This is needed to avoid accidentially creating duplicate roles for the same user/role_type/liveEvent combination,
-- which can happen because of our usage of scripts to generate roles. 

CREATE UNIQUE INDEX roles_type_user_live_event_when_active
    ON roles (role_type, user_id, live_event_id)
    WHERE status = 0;