CREATE TABLE "Events" (
	"EventId"	INTEGER NOT NULL UNIQUE,
	"Start"	TEXT NOT NULL,
	"Duration"	TEXT NOT NULL,
	"Recurrence"	INTEGER NOT NULL,
	PRIMARY KEY("EventId" AUTOINCREMENT)
);

CREATE TABLE "BlockedWebsites" (
	"BlockedSitesId" INTEGER NOT NULL UNIQUE,
	"EventId"	INTEGER NOT NULL,
	"BlockAllSites"	INTEGER NOT NULL,
	PRIMARY KEY("BlockedSitesId" AUTOINCREMENT),
	FOREIGN KEY("EventId") REFERENCES "Events"("EventId")
);

CREATE TABLE "BlockedWebsiteUrls" (
	"BlockedSitesUrlsId" INTEGER NOT NULL UNIQUE
	"BlockedSitesId"	INTEGER NOT NULL,
	"Url"	TEXT NOT NULL,
	PRIMARY KEY("BlockedSitesUrlsId" AUTOINCREMENT),
	FOREIGN KEY("BlockedSitesId") REFERENCES "BlockedWebsites"("BlockedSitesId")
)

CREATE TABLE "BlockedApplications" (
	"BlockedAppsId"	INTEGER NOT NULL UNIQUE,
	"EventId"	INTEGER NOT NULL,
	"DisplayName"	TEXT NOT NULL,
	"DisplayVersion"	TEXT NOT NULL,
	"Publisher"	TEXT NOT NULL,
	"InstallDate"	BLOB NOT NULL,
	"InstallLocation"	TEXT NOT NULL,
	"Comments"	TEXT NOT NULL,
	"UninstallString"	TEXT NOT NULL,
	"RegistryPath"	TEXT NOT NULL,
	PRIMARY KEY("BlockedAppsId" AUTOINCREMENT),
	FOREIGN KEY("EventId") REFERENCES "Events"("EventId")
);


CREATE TRIGGER PreventUrlInsertWhenBlockAllSitesTrue
BEFORE INSERT ON BlockedWebsiteUrls
FOR EACH ROW
BEGIN
	SELECT CASE
		WHEN (SELECT BlockAllSites FROM BlockedWebsites WHERE EventId = NEW.EventId) = 1 
		THEN RAISE(ABORT, 'Cannot insert URL when BlockAllSites is true')
	END;
END;

CREATE TRIGGER PreventBlockAllSitesTrueWhenUrlsExist
BEFORE UPDATE OF BlockAllSites ON BlockedWebsites
FOR EACH ROW
WHEN NEW.BlockAllSites = 1
BEGIN
    SELECT CASE
        WHEN (SELECT COUNT(*) FROM BlockedWebsiteUrls WHERE EventId = NEW.EventId) > 0 THEN
            RAISE(ABORT, 'Cannot set BlockAllSites to true when URLs are present')
    END;
END;

