ALTER TABLE users DROP CONSTRAINT users_mobile_key;

CREATE UNIQUE INDEX users_mobile_key_when_active
    ON users (mobile)
    WHERE status = 0;