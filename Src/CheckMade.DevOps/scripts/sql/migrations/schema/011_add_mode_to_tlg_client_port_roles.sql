ALTER TABLE tlg_client_port_roles RENAME TO tlg_client_port_mode_roles;

ALTER TABLE tlg_client_port_mode_roles ADD COLUMN interaction_mode SMALLINT DEFAULT -1;
ALTER TABLE tlg_client_port_mode_roles ALTER COLUMN interaction_mode SET NOT NULL;
ALTER TABLE tlg_client_port_mode_roles ALTER COLUMN interaction_mode DROP DEFAULT;