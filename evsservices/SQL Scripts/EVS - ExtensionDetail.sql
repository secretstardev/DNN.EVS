-- =========================================
-- Create table template SQL Azure Database 
-- =========================================


IF OBJECT_ID('dbo.ExtensionDetail', 'U') IS NOT NULL
	DROP TABLE dbo.ExtensionDetail
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExtensionDetail]') AND name = N'PK_ExtensionDetail')
ALTER TABLE [dbo].[ExtensionDetail] DROP CONSTRAINT [PK_ExtensionDetail]
GO

CREATE TABLE dbo.ExtensionDetail
(
	[ExtensionDetailID] INT NOT NULL IDENTITY(1,1),
	[ExtensionID] INT NOT NULL,
	[DetailID] NCHAR(36) NOT NULL, 
	[DetailName] NVARCHAR(250) NOT NULL, 
	[DetailValue] NVARCHAR(250) NOT NULL
    CONSTRAINT PK_ExtensionDetail PRIMARY KEY (ExtensionDetailID)
)
GO