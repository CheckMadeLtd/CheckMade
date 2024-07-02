CREATE UNIQUE INDEX tlg_agent_role_bindings_role_tlg_user_tlg_chat_mode_when_active
    ON tlg_agent_role_bindings (role_id, tlg_user_id, tlg_chat_id, interaction_mode)
    WHERE status = 1;