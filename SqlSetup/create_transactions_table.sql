IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LiveDocsDb')
BEGIN
  CREATE DATABASE LiveDocsDb;
END;
GO

USE [LiveDocsDb]
GO

/****** Object:  Table [dbo].[transactions]    Script Date: 04/07/2021 22:23:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[transactions](
	[TransactionId] [uniqueidentifier] NOT NULL,
	[Type] [nchar](10) NOT NULL,
	[Amount] [money] NOT NULL,
	[Status] [nchar](10) NOT NULL
) ON [PRIMARY]
GO




