-- =========================================
-- Create table template SQL Azure Database 
-- =========================================


IF OBJECT_ID('dbo.MessageType', 'U') IS NOT NULL
	DROP TABLE dbo.MessageType
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MessageType]') AND name = N'PK_MessageType')
ALTER TABLE [dbo].[MessageType] DROP CONSTRAINT [PK_MessageType]
GO

CREATE TABLE dbo.MessageType
(
	[MessageTypeID] INT NOT NULL IDENTITY(1,1), 
	[Type] NVARCHAR(50) NOT NULL
    CONSTRAINT PK_MessageType PRIMARY KEY (MessageTypeID)
)
GO

INSERT INTO dbo.MessageType ([Type])
VALUES ('Error')

INSERT INTO dbo.MessageType ([Type])
VALUES ('Warning')

INSERT INTO dbo.MessageType ([Type])
VALUES ('Info')

INSERT INTO dbo.MessageType ([Type])
VALUES ('SystemError')
