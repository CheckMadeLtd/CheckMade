ALTER TABLE tlg_agent_role_bindings
    ADD CONSTRAINT check_status_and_deactivation_date
        CHECK ((status <> 1 AND deactivation_date IS NOT NULL) OR (status = 1 AND deactivation_date IS NULL));