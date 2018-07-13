-- =========================================
-- Create table template SQL Azure Database 
-- =========================================


IF OBJECT_ID('dbo.ExtensionMessage', 'U') IS NOT NULL
	DROP TABLE dbo.ExtensionMessage
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExtensionMessage]') AND name = N'PK_ExtensionMessage')
ALTER TABLE [dbo].[ExtensionMessage] DROP CONSTRAINT [PK_ExtensionMessage]
GO

CREATE TABLE dbo.ExtensionMessage
(
	[ExtensionMessageID] INT NOT NULL IDENTITY(1,1),
	[ExtensionID] INT NOT NULL,
	[MessageTypeID] INT NOT NULL,
	[MessageID] NCHAR(36) NOT NULL, 
	[Message] NVARCHAR(250) NOT NULL, 
	[Rule] NVARCHAR(250) NOT NULL
    CONSTRAINT PK_ExtensionMessage PRIMARY KEY (ExtensionMessageID)
)
GO
