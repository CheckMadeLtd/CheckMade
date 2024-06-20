ALTER TABLE tlg_client_port_mode_roles RENAME TO tlg_agent_role_bindings;

ALTER TABLE tlg_agent_role_bindings
    RENAME CONSTRAINT tlg_client_port_roles_pkey TO tlg_agent_role_bindings_pkey;

ALTER TABLE tlg_agent_role_bindings
    RENAME CONSTRAINT tlg_client_port_roles_role_id_fkey TO tlg_agent_role_bindings_role_id_fkey;

ALTER SEQUENCE tlg_client_port_roles_id_seq RENAME TO tlg_agent_role_bindings_id_seq;