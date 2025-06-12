CREATE UNIQUE INDEX roles_type_user_live_event_when_active
    ON roles (role_type, user_id, live_event_id)
    WHERE status = 1;