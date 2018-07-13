-- =========================================
-- Create table template SQL Azure Database 
-- =========================================


IF OBJECT_ID('dbo.Extension', 'U') IS NOT NULL
	DROP TABLE dbo.Extension
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Extension]') AND name = N'PK_Extension')
ALTER TABLE [dbo].[Extension] DROP CONSTRAINT [PK_Extension]
GO

CREATE TABLE dbo.Extension
(
	[ExtensionID] INT NOT NULL IDENTITY(1,1), 
	[FileID] NCHAR(36) NOT NULL,
	[OriginalFileName] NVARCHAR(250) NOT NULL,
	[FileLocation] NVARCHAR(500) NOT NULL, 
	[ContentType] NVARCHAR(100) NOT NULL,
	[ContentSize] BIGINT NOT NULL,
	[UserKey] NCHAR(36) NOT NULL,
	[UserIP] NVARCHAR(15) NOT NULL,
	[UploadedDate] DATETIME NOT NULL,
	[ProcessedDate] DATETIME NOT NULL
    CONSTRAINT PK_Extension PRIMARY KEY (ExtensionID)
)
GO
