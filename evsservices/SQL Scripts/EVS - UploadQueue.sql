-- =========================================
-- Create table template SQL Azure Database 
-- =========================================


IF OBJECT_ID('dbo.UploadQueue', 'U') IS NOT NULL
	DROP TABLE dbo.UploadQueue
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[UploadQueue]') AND name = N'PK_UploadQueue')
ALTER TABLE [dbo].[UploadQueue] DROP CONSTRAINT [PK_UploadQueue]
GO

CREATE TABLE dbo.UploadQueue
(
	[UploadQueueID] INT NOT NULL IDENTITY(1,1),  
	[FileID] NCHAR(36) NOT NULL,
	[OriginalFileName] NVARCHAR(250) NOT NULL,
	[FileLocation] NVARCHAR(500) NOT NULL, 
	[ContentType] NVARCHAR(100) NOT NULL,
	[ContentSize] BIGINT NOT NULL,
	[UserKey] NCHAR(36) NOT NULL,
	[UserIP] NVARCHAR(15) NOT NULL,
	[UploadedDate] DATETIME NOT NULL,
	[Processing] BIT NOT NULL
    CONSTRAINT PK_UploadQueue PRIMARY KEY (UploadQueueID)
)
GO
