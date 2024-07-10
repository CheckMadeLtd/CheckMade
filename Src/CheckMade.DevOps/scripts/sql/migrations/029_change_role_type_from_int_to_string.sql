-- RoleType from now represented via IRoleType rather than RoleType Enum

ALTER TABLE roles DROP COLUMN role_type;
ALTER TABLE roles ADD COLUMN role_type varchar(6) DEFAULT '';
ALTER TABLE roles ALTER COLUMN role_type SET NOT NULL;
ALTER TABLE roles ALTER COLUMN role_type DROP DEFAULT ;
