IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditLogId      INT IDENTITY(1,1) PRIMARY KEY,
        EntityName      NVARCHAR(50)  NOT NULL,
        EntityId        INT           NOT NULL,
        Action          NVARCHAR(30)  NOT NULL,
        PerformedBy     INT           NOT NULL,
        PerformedByName NVARCHAR(100) NULL,
        Description     NVARCHAR(500) NULL,
        CreatedDate     DATETIME      NOT NULL CONSTRAINT DF_AuditLogs_CreatedDate DEFAULT (GETDATE())
    );
END
