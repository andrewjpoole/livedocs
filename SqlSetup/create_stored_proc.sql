USE [LiveDocsDb]
GO

/****** Object:  StoredProcedure [dbo].[sp_get_day2_stats]    Script Date: 04/07/2021 22:24:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_get_day2_stats]
AS
BEGIN
    SELECT Type, Count(*) as count, Sum(Amount) as sum
	FROM [LiveDocsDb].[dbo].[transactions]
	GROUP BY Type
END
GO

