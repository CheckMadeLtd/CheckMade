CREATE TABLE IF NOT EXISTS roles_to_spheres_assignments (
    id SERIAL PRIMARY KEY,
    role_id INT NOT NULL REFERENCES roles(id),
    sphere_id INT NOT NULL REFERENCES spheres_of_action(id),
    assigned_date timestamptz NOT NULL DEFAULT current_timestamp,
    unassigned_date timestamptz,
    details JSONB NOT NULL DEFAULT '{}',
    status SMALLINT NOT NULL,
    last_data_migration SMALLINT,
    CHECK ((status <> 1 AND unassigned_date IS NOT NULL) OR (status = 1 AND unassigned_date IS NULL))
);

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE roles_to_spheres_assignments TO cmappuser;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE roles_to_spheres_assignments_id_seq TO cmappuser;

CREATE UNIQUE INDEX roles_to_spheres_unique_when_active
    ON roles_to_spheres_assignments (role_id, sphere_id)
    WHERE status = 1;

CREATE INDEX roles_to_spheres_role_idx ON roles_to_spheres_assignments (role_id);
CREATE INDEX roles_to_spheres_sphere_idx ON roles_to_spheres_assignments (sphere_id);
