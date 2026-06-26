-- Creates a limited application user for runtime connections.
-- Runs once on first DB initialization as the admin superuser.

CREATE USER nexus_app WITH PASSWORD :'app_password';

GRANT CONNECT ON DATABASE nexusbank TO nexus_app;
GRANT USAGE ON SCHEMA public TO nexus_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO nexus_app;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO nexus_app;

-- Ensure future tables created by migrations are also accessible
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO nexus_app;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT USAGE ON SEQUENCES TO nexus_app;
