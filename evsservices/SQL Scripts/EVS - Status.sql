-- =========================================
-- Create table template SQL Azure Database 
-- =========================================

IF OBJECT_ID('dbo.Status', 'U') IS NOT NULL
	DROP TABLE [dbo].[Status]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Status]') AND name = N'PK_Status')
ALTER TABLE [dbo].[Status] DROP CONSTRAINT [PK_Status]
GO

CREATE TABLE [dbo].[Status]
(
	[StatusID] INT NOT NULL IDENTITY(1,1), 
	[FileID] NCHAR(36) NOT NULL,
	[UserID] NCHAR(36) NOT NULL,
	[Finished] BIT NOT NULL,
	[OverAllProgress] SMALLINT NOT NULL,
	[CurrentProgress] SMALLINT NOT NULL,
	[CurrentMessage] NVARCHAR(250) NOT NULL
    CONSTRAINT PK_Status PRIMARY KEY (StatusID)
)
GO
