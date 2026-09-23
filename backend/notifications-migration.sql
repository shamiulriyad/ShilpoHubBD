START TRANSACTION;

CREATE TABLE "UserNotifications" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Body" character varying(1000) NOT NULL,
    "Category" character varying(40) NOT NULL,
    "TargetPath" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "ReadAt" timestamp with time zone,
    CONSTRAINT "PK_UserNotifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserNotifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_UserNotifications_UserId_CreatedAt_Id" ON "UserNotifications" ("UserId", "CreatedAt", "Id");

CREATE INDEX "IX_UserNotifications_UserId_ReadAt" ON "UserNotifications" ("UserId", "ReadAt");

ALTER TABLE "UserNotifications" ENABLE ROW LEVEL SECURITY;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260922112853_AddUserNotifications', '8.0.11');

COMMIT;

