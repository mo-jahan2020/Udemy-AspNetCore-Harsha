/* ================================================================
   TextToSqlApi - Session Database Schema (SQL Server Express)
   فایل: 01_create_session_tables.sql
   ----------------------------------------------------------------
   این اسکریپت جایگزین EF Core Migration است.
   دیتابیس: TextToSqlSessions
   ----------------------------------------------------------------
   نحوه اجرا (در SQL Server Management Studio یا sqlcmd):
     sqlcmd -S localhost\SQLEXPRESS -E -i 01_create_session_tables.sql
   یا با احراز هویت SQL:
     sqlcmd -S localhost\SQLEXPRESS -U sa -P YourPassword -i 01_create_session_tables.sql
   ================================================================ */

USE master;
GO

-- اگر دیتابیس وجود نداشت، آن را ایجاد کن
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TextToSqlSessions')
BEGIN
    CREATE DATABASE TextToSqlSessions;
    PRINT 'Database TextToSqlSessions created.';
END
GO

USE TextToSqlSessions;
GO

-- ----------------------------------------------------------------
-- جدول Sessions
-- ----------------------------------------------------------------
IF OBJECT_ID(N'dbo.Sessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sessions
    (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Sessions] PRIMARY KEY,
        UserName    NVARCHAR(200)    NOT NULL,
        Title       NVARCHAR(500)    NULL,
        CreatedAt   DATETIME2        NOT NULL CONSTRAINT [DF_Sessions_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2        NOT NULL CONSTRAINT [DF_Sessions_UpdatedAt] DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_Sessions_UserName ON dbo.Sessions(UserName);
    CREATE INDEX IX_Sessions_UpdatedAt ON dbo.Sessions(UpdatedAt DESC);

    PRINT 'Table dbo.Sessions created.';
END
GO

-- ----------------------------------------------------------------
-- جدول SessionMessages
-- ----------------------------------------------------------------
IF OBJECT_ID(N'dbo.SessionMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SessionMessages
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_SessionMessages] PRIMARY KEY,
        SessionId       UNIQUEIDENTIFIER NOT NULL,
        Role            INT              NOT NULL,    -- 0 = User, 1 = Assistant
        Content         NVARCHAR(MAX)   NOT NULL,
        GeneratedSql    NVARCHAR(MAX)    NULL,
        ModelName       NVARCHAR(100)    NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT [DF_SessionMessages_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        ResponseTimeMs  BIGINT           NULL,
        CONSTRAINT [FK_SessionMessages_Sessions_SessionId]
            FOREIGN KEY (SessionId) REFERENCES dbo.Sessions(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_SessionMessages_SessionId ON dbo.SessionMessages(SessionId);
    CREATE INDEX IX_SessionMessages_CreatedAt ON dbo.SessionMessages(CreatedAt);

    PRINT 'Table dbo.SessionMessages created.';
END
GO

-- ----------------------------------------------------------------
-- (اختیاری) داده نمونه برای تست
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Sessions)
BEGIN
    DECLARE @sid UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Sessions (Id, UserName, Title)
    VALUES (@sid, N'test-user', N'Sample session');

    INSERT INTO dbo.SessionMessages (Id, SessionId, Role, Content, GeneratedSql, ModelName, ResponseTimeMs)
    VALUES
        (NEWID(), @sid, 0, N'فروش ماهانه را نشان بده', NULL, NULL, NULL),
        (NEWID(), @sid, 1, N'SELECT TOP 100 * FROM Sales', N'SELECT TOP 100 * FROM Sales', N'qwen2.5-coder', 850);

    PRINT 'Sample data inserted.';
END
GO

PRINT '=== Schema creation completed successfully ===';
GO
